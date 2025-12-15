using Connect.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Connect.Generator
{
    public class LevelGenerator : MonoBehaviour
    {
        #region START_METHODS

        [SerializeField] private bool canGeneratorOnce;

        [SerializeField] private int stage;

        public int levelSizeX;
        public int levelSizeY;
        private void Awake()
        {
            SpawnBoard();
            SpawnNodes();
        }

        [SerializeField] private SpriteRenderer _boardPrefab, _bgCellPrefab;

        private void SpawnBoard()
        {
            // 1️⃣ Tính tâm của level
            float centerX = levelSizeX / 2f;
            float centerY = levelSizeY / 2f;

            // 2️⃣ Spawn board ở giữa
            var board = Instantiate(
                _boardPrefab,
                new Vector3(centerX, centerY, 0f),
                Quaternion.identity
            );

            // 3️⃣ Set size board theo X/Y
            board.size = new Vector2(
                levelSizeX + 0.08f,
                levelSizeY + 0.08f
            );

            // 4️⃣ Spawn background cells
            for (int x = 0; x < levelSizeX; x++)
            {
                for (int y = 0; y < levelSizeY; y++)
                {
                    Instantiate(
                        _bgCellPrefab,
                        new Vector3(x + 0.5f, y + 0.5f, 0f),
                        Quaternion.identity
                    );
                }
            }

            // 5️⃣ Setup camera
            Camera cam = Camera.main;

            // Lấy cạnh lớn hơn để camera fit toàn bộ board
            float maxSize = Mathf.Max(levelSizeX, levelSizeY);

            cam.orthographicSize = maxSize / 1.6f + 1f;
            cam.transform.position = new Vector3(centerX, centerY, -10f);
        }

        [SerializeField] private NodeRenderer _nodePrefab;

        public Dictionary<Point, NodeRenderer> nodeGrid;
        private NodeRenderer[,] nodeArray;

        private void SpawnNodes()
        {
            nodeGrid = new Dictionary<Point, NodeRenderer>();
            nodeArray = new NodeRenderer[levelSizeX, levelSizeY];
            Vector3 spawnPos;
            NodeRenderer spawnedNode;

            for (int i = 0; i < levelSizeX; i++)
            {
                for (int j = 0; j < levelSizeY; j++)
                {
                    spawnPos = new Vector3(i + 0.5f, j + 0.5f, 0f);
                    spawnedNode = Instantiate(_nodePrefab, spawnPos, Quaternion.identity);
                    spawnedNode.Init();
                    nodeGrid.Add(new Point(i, j), spawnedNode);
                    nodeArray[i, j] = spawnedNode;
                    spawnedNode.gameObject.name = i.ToString() + j.ToString();
                }
            }
        }


        #endregion

        #region BUTTON_FUNCTION

        [SerializeField] private GameObject _simulateButton;

        public void ClickedSimulate()
        {
            Levels = new Dictionary<string, LevelData>();

            foreach (var item in _allLevelList.Levels)
            {
                Levels[item.LevelName] = item;
            }
            GenerateDefault();

            _simulateButton.SetActive(false);
        }

        [SerializeField] private LevelList _allLevelList;
        private Dictionary<string, LevelData> Levels;

        #region GENERATE_SINGLE_LEVEL
        private void GenerateDefault()
        {
            GenerateLevelData();
        }

        public LevelData currentLevelData;

        private void GenerateLevelData(int level = 0)
        {
            string currentLevelName = "Level" + stage.ToString() + level.ToString();

            if (!Levels.ContainsKey(currentLevelName))
            {
#if UNITY_EDITOR
                currentLevelData = ScriptableObject.CreateInstance<LevelData>();
                AssetDatabase.CreateAsset(currentLevelData, "Assets/Common/Prefabs/Levels/" +
                    currentLevelName + ".asset");
                AssetDatabase.SaveAssets();
#endif
                Levels[currentLevelName] = currentLevelData;
                _allLevelList.Levels.Add(currentLevelData);
            }

            currentLevelData = Levels[currentLevelName];
            currentLevelData.LevelName = currentLevelName;
            GetComponent<GenerateMethod>().Generate();
        }

        #endregion

        #region NODE_RENDERING

        private List<Point> directions = new List<Point>()
        { Point.up,Point.down,Point.left,Point.right};

        public void RenderGrid(Dictionary<Point, int> grid)
        {
            int currentColor;
            int numOfConnectedNodes;

            foreach (var item in nodeGrid)
            {
                item.Value.Init();
                currentColor = grid[item.Key];
                numOfConnectedNodes = 0;

                if (currentColor != -1)
                {
                    foreach (var direction in directions)
                    {
                        if (grid.ContainsKey(item.Key + direction) &&
                            grid[item.Key + direction] == currentColor)
                        {
                            item.Value.SetEdge(currentColor, direction);
                            numOfConnectedNodes++;
                        }
                    }

                    if (numOfConnectedNodes <= 1)
                    {
                        item.Value.SetEdge(currentColor, Point.zero);
                    }
                }
            }
        }

        private Point[] neighbourPoints = new Point[]
        {
            Point.up,Point.left,Point.down, Point.right
        };
        #endregion

        #endregion
    }

    public interface GenerateMethod
    {
        public void Generate();
    }


}
