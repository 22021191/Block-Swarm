using UnityEngine;
using Connect.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
namespace Connect.Generator
{
    public class LevelGenerator : MonoBehaviour
    {
        [Header("Level Size")]
        [SerializeField] private int levelIndex;
        [SerializeField] private int levelSizeX;
        [SerializeField] private int levelSizeY;

        [Header("Board")]
        [SerializeField] private SpriteRenderer boardPrefab;
        [SerializeField] private SpriteRenderer bgCellPrefab;
        [SerializeField] private float boardPadding = 0.08f;

        private LevelData currentLevelData;

        private void Awake()
        {
            SpawnBoard();
        }

        // ================= BOARD =================
        private void SpawnBoard()
        {
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
                    Instantiate(
                        bgCellPrefab,
                        new Vector3(x + 0.5f, y + 0.5f, 0),
                        Quaternion.identity,
                        transform
                    );
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

        // ================= GENERATE =================
        public void GenerateAndSave()
        {
            CreateLevelData();

            if (!TryGetComponent<GenerateMethod>(out var generator))
            {
                Debug.LogError("GenerateMethod not found!");
                return;
            }

            generator.Generate(currentLevelData);

            LevelJsonUtility.Save(currentLevelData);
        }

        private void CreateLevelData()
        {
            currentLevelData = new LevelData
            {
                LevelName = $"Level_{levelIndex}",
                width = levelSizeX,
                height = levelSizeY
            };
        }
    }
    public interface GenerateMethod
    {
        void Generate(LevelData levelData);
    }
}
