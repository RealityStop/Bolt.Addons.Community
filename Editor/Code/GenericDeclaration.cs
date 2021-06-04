using Unity.VisualScripting;
using System;
using Bolt.Addons.Integrations.Continuum;

namespace Bolt.Addons.Community.Code.Editor
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
