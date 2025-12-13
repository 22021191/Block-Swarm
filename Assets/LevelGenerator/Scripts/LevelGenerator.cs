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

        public int levelSize => stage + 4;

        private void Awake()
        {
            SpawnBoard();

        }

        [SerializeField] private SpriteRenderer _boardPrefab, _bgCellPrefab;

        private void SpawnBoard()
        {
            var board = Instantiate(_boardPrefab,
                new Vector3(levelSize / 2f, levelSize / 2f, 0f),
                Quaternion.identity);

            board.size = new Vector2(levelSize + 0.08f, levelSize + 0.08f);

            for (int i = 0; i < levelSize; i++)
            {
                for (int j = 0; j < levelSize; j++)
                {
                    Instantiate(_bgCellPrefab, new Vector3(i + 0.5f, j + 0.5f, 0f), Quaternion.identity);
                }
            }

            Camera.main.orthographicSize = levelSize / 1.6f + 1f;
            Camera.main.transform.position = new Vector3(levelSize / 2f, levelSize / 2f, -10f);
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
            currentLevelData.Edges = new List<Edge>();

            GetComponent<GenerateMethod>().Generate();
        }


        #endregion
    }
        #endregion
    public interface GenerateMethod
    {
        public void Generate();
    }
}
