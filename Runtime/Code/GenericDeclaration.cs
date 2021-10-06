using Unity.VisualScripting;
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    [Inspectable]
    [Serializable]
    [RenamedFrom("Bolt.Addons.Community.Code.GenericDeclaration")]
    public sealed class GenericDeclaration
    {
        [Inspectable]
        public string name;
        [Inspectable]
        public SystemType type = new SystemType() { type = typeof(int) };
        [Inspectable]
        public SystemType constraint = new SystemType() { type = typeof(object) };
    }
}
