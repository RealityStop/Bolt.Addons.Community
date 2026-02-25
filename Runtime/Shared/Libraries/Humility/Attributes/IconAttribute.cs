using System;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class IconAttribute : Attribute
    {
        public string cacheCategory;
        public string fileName;
        public string pathSearchObjectName;

        public IconAttribute(string cacheCategory, string fileName, string pathSearchObjectName)
        {
            this.cacheCategory = cacheCategory;
            this.fileName = fileName;
            this.pathSearchObjectName = pathSearchObjectName;
        }
    }
}
