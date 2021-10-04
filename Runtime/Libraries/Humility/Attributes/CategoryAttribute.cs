using System;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class CategoryAttribute : Attribute
    {
        public string path;

        public CategoryAttribute(string path)
        {
            this.path = path;
        }
    }
}
