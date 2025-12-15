using System.Collections;
using System.Collections.Generic;
namespace Connect.Generator
{
    public class GridNode
    {
        public GridNode Prev;
        public GridData Data;
        private int neighborIndex, emptyIndex;
        private List<Point> neighbors, emptyPositions;
        public GridNode(int LevelSizeX,int LevelSizeY)
        {
            Prev = null;
            Data = new GridData(LevelSizeX, LevelSizeY);
            neighbors = new List<Point>();
            emptyPositions = new List<Point>();
            neighborIndex = 0;
            emptyIndex = 0;
            for (int i = 0; i < LevelSizeX; i++)
            {
                for (int j = 0; j < LevelSizeY; j++)
                {
                    emptyPositions.Add(new Point(i, j));
                }
            }
            Shuffle(emptyPositions);
        }

        public GridNode(GridData data, GridNode prev = null)
        {
            Data = data;
            Prev = prev;
            neighborIndex = 0;
            emptyIndex = 0;
            neighbors = new List<Point>();
            emptyPositions = new List<Point>();
            Data.GetResultsList(neighbors, emptyPositions);
            Shuffle(neighbors);
            Shuffle(emptyPositions);
        }

        public GridNode(int levelSizeX, int levelSizeY, HashSet<Point> points)
        {
            Prev = null;
            Data = new GridData(levelSizeX, levelSizeY, points);
            neighborIndex = 0;
            emptyIndex = 0;
            neighbors = new List<Point>();
            emptyPositions = new List<Point>();

            foreach (var item in points)
            {
                emptyPositions.Add(item);
            }

            Shuffle(emptyPositions);
        }

        public static void Shuffle(List<Point> list)
        {
            System.Random rng = new System.Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}