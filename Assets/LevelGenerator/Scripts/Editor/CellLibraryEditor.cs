using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
namespace Connect.Generator
{
    [CreateAssetMenu(fileName = "CellLibraryEditor", menuName = "Scriptable Objects/CellLibraryEditor")]
    public class CellLibraryEditor : ScriptableObject
    {
        public List<CellEntry> cells;

        public GameObject GetPrefab(BlockType type)
        {
            return cells.Find(c => c.type == type)?.prefab;
        }
    }
    [System.Serializable]
    public class CellEntry
    {
        public BlockType type;
        public GameObject prefab;
    }
}