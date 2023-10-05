using Unity.VisualScripting.Community.Libraries.CSharp;
using System;
using UnityEngine;
using Unity.Plastic.Newtonsoft.Json;

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

        [SerializeField]
        public SystemType Paramtype = new SystemType();

        [Serialize]
        [Inspectable]
        public string name;

        public bool hasDefault;

        [SerializeField]
        public string stringValue;

        [SerializeField]
        public int intValue;

        [SerializeField]
        public float floatValue;

        [SerializeField]
        public bool boolValue;

        [SerializeField]
        public int EnumValue = 0;

        [Serialize]
        [SerializeField]
        public object defaultValue;

        public ParameterModifier modifier = ParameterModifier.None;
    }
}
