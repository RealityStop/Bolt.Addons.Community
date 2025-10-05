using Unity.VisualScripting;
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;

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
        public SystemType type = new SystemType(typeof(object));
        [Inspectable]
        public bool get = true;
        [Inspectable]
        public bool set = true;
#if UNITY_EDITOR
        public bool isOpen;
#endif
    }
}