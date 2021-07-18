using Unity.VisualScripting;
using System;

namespace Bolt.Addons.Community.Code
{
    [Serializable]
    [Inspectable]
    public sealed class InterfacePropertyItem
    {
        [Inspectable]
        public string name;
        [Inspectable]
        public Type type = typeof(object);
        [Inspectable]
        public bool get = true;
        [Inspectable]
        public bool set = true;
    }
}