using Unity.VisualScripting;
using System;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    [Inspectable]
    [RenamedFrom("Bolt.Addons.Community.Code.InterfacePropertyItem")]
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