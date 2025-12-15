using System.Collections.Generic;
using UnityEngine;
namespace Connect.Generator
{
    public class GridData
    {
        private static Point[] directionChecks = new Point[]
        { Point.up,Point.down,Point.left,Point.right };

        public int[,] _grid;
        public bool IsSolved;
        public Point CurrentPos;
        public int ColorId;
        public static int LevelSizeX, LevelSizeY;

        public GridData(int levelSizeX, int levelSizeY)
        {
            _grid = new int[levelSizeX, levelSizeY];

            for (int i = 0; i < levelSizeX; i++)
            {
                for (int j = 0; j < levelSizeY; j++)
                {
                    _grid[i, j] = -1;
                }
            }

            IsSolved = false;
            ColorId = -1;
            LevelSizeX = levelSizeX;
            LevelSizeY = levelSizeY;
        }

        public GridData(int i, int j, int passedColor, GridData gridCopy)
        {
            _grid = new int[LevelSizeX, LevelSizeY];

            for (int a = 0; a < LevelSizeX; a++)
            {
                for (int b = 0; b < LevelSizeY; b++)
                {
                    _grid[a, b] = gridCopy._grid[a, b];
                }
            }

            ColorId = gridCopy.ColorId;

            CurrentPos = new Point(i, j);
            ColorId = passedColor;
            _grid[CurrentPos.x, CurrentPos.y] = ColorId;
            IsSolved = false;
        }

        public GridData(int levelSizeX, int levelSizeY, HashSet<Point> points)
        {
            _grid = new int[levelSizeX, levelSizeY];

            for (int i = 0; i < levelSizeX; i++)
            {
                for (int j = 0; j < levelSizeY; j++)
                {
                    _grid[i, j] = -2;
                }
            }

            foreach (var point in points)
            {
                _grid[point.x, point.y] = -1;
            }

            IsSolved = false;
            ColorId = -1;
            LevelSizeX = levelSizeX;
            LevelSizeY = levelSizeY;
        }

        public bool IsInsideGrid(Point pos)
        {
            return pos.IsPointValid(LevelSizeX, LevelSizeY);
        }

        public bool IsGridComplete()
        {
            foreach (var item in _grid)
            {
                if (item == -1) return false;
            }

            for (int i = 0; i <= ColorId; i++)
            {
                int result = 0;

                foreach (var item in _grid)
                {
                    if (item == i)
                        result++;
                }

                if (result < 3)
                    return false;

            }

            return true;
        }

        public bool IsNotNeighbour(Point pos)
        {

            for (int i = 0; i < LevelSizeX; i++)
            {
                for (int j = 0; j < LevelSizeY; j++)
                {
                    if (_grid[i, j] == ColorId && new Point(i, j) != CurrentPos)
                    {
                        for (int p = 0; p < directionChecks.Length; p++)
                        {
                            if (pos - new Point(i, j) == directionChecks[p])
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        public int FlowLength()
        {
            int result = 0;
            foreach (var item in _grid)
            {
                if (item == ColorId)
                    result++;
            }

            return result;
        }

        public void GetResultsList(List<Point> neighbors, List<Point> emptyPositions)
        {
            int[,] emptyGrid = new int[LevelSizeX, LevelSizeY];
            for (int i = 0; i < LevelSizeX; i++)
            {
                for (int j = 0; j < LevelSizeY; j++)
                {
                    emptyGrid[i, j] = -1;
                }
            }

            for (int i = 0; i < LevelSizeX; i++)
            {
                for (int j = 0; j < LevelSizeY; j++)
                {
                    if (_grid[i, j] == -1)
                    {
                        emptyGrid[i, j] = 0;
                        for (int k = 0; k < directionChecks.Length; k++)
                        {
                            Point tempPoint = new Point(directionChecks[k].x + i, directionChecks[k].y + j);
                            if (IsInsideGrid(tempPoint) && _grid[tempPoint.x, tempPoint.y] == -1)
                            {
                                emptyGrid[i, j]++;
                            }
                        }
                    }
                }
            }

            List<Point> zeroNeighbours = new List<Point>();
            List<Point> allNeighbours = new List<Point>();

            for (int i = 0; i < directionChecks.Length; i++)
            {
                Point tempPoint = CurrentPos + directionChecks[i];
                if (IsInsideGrid(tempPoint) &&
                    IsNotNeighbour(tempPoint) &&
                    emptyGrid[tempPoint.x, tempPoint.y] != -1)
                {
                    if (emptyGrid[tempPoint.x, tempPoint.y] == 0)
                    {
                        zeroNeighbours.Add(tempPoint);
                        emptyGrid[tempPoint.x, tempPoint.y] = -1;
                    }
                    allNeighbours.Add(tempPoint);
                }
            }

            List<Point> zeroEmpty = new List<Point>();
            List<Point> oneEmpty = new List<Point>();
            List<Point> allEmpty = new List<Point>();

            for (int i = 0; i < LevelSizeX; i++)
            {
                for (int j = 0; j < LevelSizeY; j++)
                {
                    if (emptyGrid[i, j] == 0)
                    {
                        zeroEmpty.Add(new Point(i, j));
                    }

                    if (emptyGrid[i, j] == 1)
                    {
                        oneEmpty.Add(new Point(i, j));
                    }

                    if (emptyGrid[i, j] != -1)
                    {
                        allEmpty.Add(new Point(i, j));
                    }
                }
            }

            List<HashSet<Point>> connectedSet = new List<HashSet<Point>>();
            HashSet<Point> minSet = FindMinConnectedSet(new List<Point>(allEmpty), connectedSet);
            List<HashSet<Point>> tempSet = new List<HashSet<Point>>();

            foreach (var item in connectedSet)
            {
                bool canAdd = true;

                foreach (var neighbor in allNeighbours)
                {
                    if (item.Contains(neighbor))
                        canAdd = false;
                }
                if (canAdd)
                {
                    tempSet.Add(item);
                }
            }
            connectedSet = tempSet;

            if (zeroEmpty.Count > 0 || zeroNeighbours.Count > 1)
            {
                return;
            }

            if (zeroNeighbours.Count == 1)
            {
                neighbors.Add(zeroNeighbours[0]);
                return;
            }

            foreach (var item in allNeighbours)
            {
                neighbors.Add(item);
            }

            if (FlowLength() < 3) return;

            if (oneEmpty.Count > 0)
            {
                foreach (var item in oneEmpty)
                {
                    if (minSet.Contains(item))
                        emptyPositions.Add(item);
                }

                return;
            }

            foreach (var item in allEmpty)
            {
                if (minSet.Contains(item))
                    emptyPositions.Add(item);
            }

        }

        public static HashSet<Point> FindMinConnectedSet(List<Point> points, List<HashSet<Point>> connectedSet)
        {
            HashSet<Point> visited = new HashSet<Point>();
            HashSet<Point> allPoints = new HashSet<Point>(points);

            foreach (var point in points)
            {
                if (!visited.Contains(point))
                {
                    HashSet<Point> connected = new HashSet<Point>();
                    Queue<Point> queue = new Queue<Point>();

                    queue.Enqueue(point);

                    while (queue.Count > 0)
                    {
                        Point current = queue.Dequeue();

                        if (!visited.Contains(current))
                        {
                            connected.Add(current);
                            visited.Add(current);

                            foreach (var neighbor in GetNeighbors(current))
                            {
                                if (!visited.Contains(neighbor) && allPoints.Contains(neighbor))
                                {
                                    queue.Enqueue(neighbor);
                                }
                            }
                        }
                    }

                    connectedSet.Add(connected);
                }
            }

            HashSet<Point> minSet = null;

            foreach (var item in connectedSet)
            {
                if (minSet == null || item.Count < minSet.Count)
                {
                    minSet = item;
                }
            }

            return minSet;
        }

        private static List<Point> GetNeighbors(Point point)
        {
            List<Point> result = new List<Point>
            {
                new Point(point.x, point.y + 1),
                new Point(point.x, point.y - 1),
                new Point(point.x + 1, point.y),
                new Point(point.x - 1, point.y)
            };

            return result;
        }
    }
}