using Unity.VisualScripting.Community.Libraries.CSharp;
using System;
using UnityEngine;
using Unity.Plastic.Newtonsoft.Json;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;

namespace Unity.VisualScripting.Community.Utility
{
    [Serializable]
    [Inspectable]
    [RenamedFrom("Bolt.Addons.Community.Utility.TypeParam")]
    public sealed class TypeParam : ISerializationCallbackReceiver
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
        public bool isParamsParameter;

        [Serialize]
        [SerializeField]
        public object defaultValue;

        [Serialize]
        [SerializeField]
        public string defaultValueSerialized;

        [SerializeField]
        [Serialize]
        private string defaultValueType;

        public ParameterModifier modifier = ParameterModifier.None;

        public object GetDefaultValue()
        {
            if (!string.IsNullOrEmpty(defaultValueSerialized))
            {
                return JsonConvert.DeserializeObject(defaultValueSerialized, Type.GetType(defaultValueType));
            }

            return defaultValue;
        }

        public void OnBeforeSerialize()
        {
            defaultValueSerialized = JsonConvert.SerializeObject(defaultValue);
            defaultValueType = defaultValue.GetType().AssemblyQualifiedName;
        }

        public void OnAfterDeserialize()
        {
        }
    }
}