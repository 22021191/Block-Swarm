using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Connect.Core
{
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
    public class ResourceAttribute : Attribute
    {
        public enum ResourceType
        {
            Unspecified,
            Material
        }

        public ResourceType Type { get; set; }
        public string Path { get; set; }
    }
}