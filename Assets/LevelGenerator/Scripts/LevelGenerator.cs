using UnityEngine;
using UnityEditor;
using Connect.Common;
using System.Collections.Generic;

namespace Connect.Generator
{
    [ExecuteAlways]
    public class LevelGenerator : MonoBehaviour
    {
        [Header("Level Info")]
        public int levelIndex = 1;
        public int _width = 40;
        public int _height = 40;

        [Header("Grid Info")]
        public float cellSize = 1f;
        public Color gridColor = Color.green;

        [Header("Prefabs")]
        public GameObject[] blockPrefabs; 

        private LevelData currentLevelData;
        private BlockType[,] gridData;
        private void OnEnable()
        {
            InitGrid();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _width = Mathf.Max(1, _width);
            _height = Mathf.Max(1, _height);
            cellSize = Mathf.Max(0.1f, cellSize);

            InitGrid();
            SceneView.RepaintAll();
        }
#endif

        private void InitGrid()
        {
            gridData = new BlockType[_width, _height];
        }

        // ================= GRID DRAW =================
        private void OnDrawGizmos()
        {
            Gizmos.color = gridColor;
            Vector3 origin = GetOrigin();

            for (int x = 0; x <= _width; x++)
            {
                Gizmos.DrawLine(
                    origin + new Vector3(x * cellSize, 0),
                    origin + new Vector3(x * cellSize, _height * cellSize)
                );
            }

            for (int y = 0; y <= _height; y++)
            {
                Gizmos.DrawLine(
                    origin + new Vector3(0, y * cellSize),
                    origin + new Vector3(_width * cellSize, y * cellSize)
                );
            }
        }

        // ================= GRID MATH =================
        public Vector3 GetOrigin()
        {
            return transform.position
                   - new Vector3(_width * cellSize / 2f, _height * cellSize / 2f, 0f);
        }

        public bool WorldToGrid(Vector3 worldPos, out int x, out int y)
        {
            Vector3 local = worldPos - GetOrigin();

            x = Mathf.FloorToInt(local.x / cellSize);
            y = Mathf.FloorToInt(local.y / cellSize);

            return x >= 0 && x < _width && y >= 0 && y < _height;
        }

        public Vector3 GridToWorldCenter(int x, int y)
        {
            return GetOrigin() + new Vector3(
                (x + 0.5f) * cellSize,
                (y + 0.5f) * cellSize,
                0f
            );
        }

        // ================= PAINT =================
#if UNITY_EDITOR
        public void PaintCell(int x, int y, BlockType type)
        {
            if (gridData[x, y] == type)
                return;

            Undo.RecordObject(this, "Paint Grid Cell");

            gridData[x, y] = type;
            SpawnBlock(x, y, type);

            EditorUtility.SetDirty(this);
        }
#endif

        // ================= SPAWN =================
        private void SpawnBlock(int x, int y, BlockType type)
        {
            int index = (int)type;
            if (index < 0 || index >= blockPrefabs.Length)
                return;

            GameObject prefab = blockPrefabs[index];
            if (prefab == null)
                return;

#if UNITY_EDITOR
            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(obj, "Spawn Block");
#else
            GameObject obj = Instantiate(prefab);
#endif
            obj.transform.position = GridToWorldCenter(x, y);
            obj.transform.SetParent(transform);
            obj.name = $"Block_{type}_{x}_{y}";
        }

        // ================= CLEAR =================
        [ContextMenu("Clear Board")]
        public void ClearBoard()
        {
#if UNITY_EDITOR
            Undo.RecordObject(this, "Clear Board");
#endif
            foreach (Transform child in transform)
            {
#if UNITY_EDITOR
                Undo.DestroyObjectImmediate(child.gameObject);
#else
                Destroy(child.gameObject);
#endif
            }
            InitGrid();
        }

        // ================= SAVE LEVEL =================
        [ContextMenu("Save Level")]
        public void SaveLevel()
        {
            currentLevelData = new LevelData
            {
                LevelName = $"Level_{levelIndex}",
                width = _width,
                height = _height,
                cells = new List<CellInfo>()
            };

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (gridData[x, y] == BlockType.None)
                        continue;

                    currentLevelData.cells.Add(new CellInfo
                    {
                        x = x,
                        y = y,
                        type = gridData[x, y]
                    });
                }
            }

            LevelJsonUtility.Save(currentLevelData);
        }
    }
}
