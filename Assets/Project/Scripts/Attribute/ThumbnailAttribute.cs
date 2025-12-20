using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Connect.Core
{
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
    public class ThumbnailAttribute : Attribute
    {
        public string Key { get; set; }
        public string Name { get; set; }
    }
}