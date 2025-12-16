using System.IO;
using UnityEngine;
using Connect.Common;
namespace Connect.Generator
{

    public static class LevelJsonUtility
    {
        private static string FolderPath =>
            Application.dataPath + "/LevelsJson";

        // ================= SAVE =================
        public static void Save(LevelData data)
        {
            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);

            string json = JsonUtility.ToJson(data, true);
            string path = $"{FolderPath}/{data.LevelName}.json";

            File.WriteAllText(path, json);

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif

            Debug.Log($"[LevelJson] Saved: {path}");
        }

        // ================= LOAD =================
        public static LevelData Load(string levelName)
        {
            string path = $"{FolderPath}/{levelName}.json";

            if (!File.Exists(path))
            {
                Debug.LogError($"[LevelJson] Not found: {path}");
                return null;
            }

            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<LevelData>(json);
        }
    }

}