using Bolt.Addons.Libraries.CSharp;
using Bolt.Addons.Libraries.Humility;
using System;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Utility
{
    [Serializable]
    [Inspectable]
    public sealed class TypeParam
    {
        [Serialize]
        [Inspectable]
        public Type type;

        [Serialize]
        [Inspectable]
        public string name;

        public bool hasDefault;

        [Serialize]
        public object defaultValue;

        public ParameterModifier modifier = ParameterModifier.None;
    }
}
