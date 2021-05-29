using System;

namespace Bolt.Addons.Integrations.Continuum.Humility
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public sealed class DisplayNameAttribute : Attribute
    {
        public string label;

        public DisplayNameAttribute(string label)
        {
            this.label = label;
        }
    }
}
