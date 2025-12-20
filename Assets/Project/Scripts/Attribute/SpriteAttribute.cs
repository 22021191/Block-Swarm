using System;

namespace Connect.Core
{
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
    public class SpriteAttribute : Attribute
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string NamePattern { get; set; }
    }
}