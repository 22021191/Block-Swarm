using UnityEngine;
using Connect.Common;
namespace Connect.Generator
{
    [ExecuteAlways]
    public class LevelGenerator : MonoBehaviour
    {
        [Header("Level Info")]
        public int levelIndex;
        public int levelSizeX;
        public int levelSizeY;

        [Header("Prefabs")]
        public SpriteRenderer boardPrefab;
        public CellData bgCellPrefab;
         [SerializeField] private float boardPadding = 0.08f;

        private LevelData currentLevelData;
        public void SpawnBoard()
        {
            ClearBoard();
            float centerX = levelSizeX / 2f;
            float centerY = levelSizeY / 2f;
            var board = Instantiate(
                boardPrefab,
                new Vector3(centerX, centerY, 0),
                Quaternion.identity,
                transform
            );

            board.size = new Vector2(
                levelSizeX + boardPadding,
                levelSizeY + boardPadding
            );

            for (int x = 0; x < levelSizeX; x++)
            {
                for (int y = 0; y < levelSizeY; y++)
                {
                    var cellObj = Instantiate(
                        bgCellPrefab,
                        new Vector3(x + 0.5f, y + 0.5f, 0),
                        Quaternion.identity,
                        transform
                    );

                    var cell = cellObj.GetComponent<CellData>();
                    cell.x = x;
                    cell.y = y;
                }
            }
             SetupCamera(centerX, centerY);
        }
        private void SetupCamera(float centerX, float centerY)
        {
            Camera cam = Camera.main;
            float maxSize = Mathf.Max(levelSizeX, levelSizeY);

            cam.orthographicSize = maxSize / 2f + 0.5f;
            cam.transform.position = new Vector3(centerX, centerY, -10f);
        }
        public void ClearBoard()
        {
            while (transform.childCount > 0)
                DestroyImmediate(transform.GetChild(0).gameObject);
        }

        [ContextMenu("Save Level")]
        public void SaveLevel()
        {
            currentLevelData = new LevelData
            {
                LevelName = $"Level_{levelIndex}",
                width = levelSizeX,
                height = levelSizeY
            };
            currentLevelData.cells = new System.Collections.Generic.List<CellInfo>();
            foreach (var cell in GetComponentsInChildren<CellData>())
            {
                currentLevelData.cells.Add(new CellInfo
                {
                    x = cell.x,
                    y = cell.y,
                    type = cell.type
                });
            }

            LevelJsonUtility.Save(currentLevelData);
        }
    }
}