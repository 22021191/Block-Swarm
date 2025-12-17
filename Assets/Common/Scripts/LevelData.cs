using System;
using System.Collections.Generic;
using UnityEngine;

namespace Connect.Common
{
    [Serializable]
    public class LevelData
    {
        public string LevelName;
        public int width;
        public int height;

        public List<CellInfo> cells = new List<CellInfo>();
    }
    [System.Serializable]
    public class CellInfo
    {
        public int x;
        public int y;
        public BlockType type;
    }
}
