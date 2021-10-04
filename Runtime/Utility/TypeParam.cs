using Unity.VisualScripting.Community.Libraries.CSharp;
using System;

namespace Unity.VisualScripting.Community.Utility
{
    [Serializable]
    [Inspectable]
    [RenamedFrom("Bolt.Addons.Community.Utility.TypeParam")]
    public sealed class TypeParam
    { 
        [Serialize]
        [Inspectable]
        public Type type = typeof(object);

        [Serialize]
        [Inspectable]
        public string name;

        public bool hasDefault;

        [Serialize]
        public object defaultValue;

        public ParameterModifier modifier = ParameterModifier.None;
    }
}
