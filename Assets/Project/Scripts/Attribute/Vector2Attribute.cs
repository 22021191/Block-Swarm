using System;
using UnityEngine;
namespace Connect.Core
{
    public class Vector2Attribute : Attribute
    {
        public float X { get; set; }
        public float Y { get; set; }

        #region properties
        public Vector2 AsVector2
        {
            get
            {
                return new Vector2(this.X, this.Y);
            }
        }
        #endregion
    }
}