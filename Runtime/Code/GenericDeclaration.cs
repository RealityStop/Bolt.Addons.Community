using Unity.VisualScripting;
using System;
using Bolt.Addons.Libraries.CSharp;

namespace Bolt.Addons.Community.Code
{
    [Inspectable]
    [Serializable]
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
