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

        public List<CellData> cells = new List<CellData>();
    }

    [Serializable]
    public class CellData
    {
        public int x;
        public int y;
        public int Id;
    }
}
