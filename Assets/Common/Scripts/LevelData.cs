using System.Collections.Generic;
using UnityEngine;

namespace Connect.Common
{
    [CreateAssetMenu(fileName = "Level",menuName = "SO/Level")]
    public class LevelData : ScriptableObject
    {
        public string LevelName;
    }
}
