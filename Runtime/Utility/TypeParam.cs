using Unity.VisualScripting.Community.Libraries.CSharp;
using System;
using UnityEngine;
using Unity.Plastic.Newtonsoft.Json;

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
        private string defaultValueSerialized;
        
        [SerializeField]
        [Serialize]
        private string defaultValueType;

        public ParameterModifier modifier = ParameterModifier.None;

        public object GetDefaultValue()
        {
            if (!string.IsNullOrEmpty(defaultValueSerialized) && !string.IsNullOrEmpty(defaultValueType))
            {
                return JsonConvert.DeserializeObject(defaultValueSerialized, Type.GetType(defaultValueType));
            }

            return defaultValue;
        }

        public void OnBeforeSerialize()
        {
            if (defaultValue != null) 
            { 
                defaultValueSerialized = JsonConvert.SerializeObject(defaultValue);
                defaultValueType = defaultValue.GetType().AssemblyQualifiedName;
            }
        }
        
        public void OnAfterDeserialize()
        {
        }
    }
}