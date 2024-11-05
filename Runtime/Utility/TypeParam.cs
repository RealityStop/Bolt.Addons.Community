using Unity.VisualScripting.Community.Libraries.CSharp;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Unity.VisualScripting.Community.Utility
{
    [Serializable]
    [Inspectable]
    [RenamedFrom("Bolt.Addons.Community.Utility.TypeParam")]
    public sealed class TypeParam : ISerializationCallbackReceiver
    {
        [SerializeField]
        [Inspectable]
        public Type type = typeof(object);

        [SerializeField]
        public SystemType Paramtype = new SystemType();

        [SerializeField]
        [Inspectable]
        public string name;

        [InspectableIf("showDefault")]
        public bool hasDefault;

        [InspectableIf("showCall")]
        public bool useInCall;

        public bool usesGeneric { get; }

        public int generic;

        [SerializeField]
        public bool isParamsParameter;

        [Serialize]
        [SerializeField]
        [InspectorToggleLeft]
        public object defaultValue;
        public SerializableType typeHandle;
        public bool showCall = false;
        public bool showDefault = false;

#if UNITY_EDITOR
        public bool attributesOpened;
        public bool opened;
#endif

        [SerializeField]
        private SerializationData defaultValueSerialized;
        [SerializeField]
        private SerializationData attributesSerialized;

        [Inspectable]
        public ParameterModifier modifier = ParameterModifier.None;

        [NonSerialized]
        private List<AttributeDeclaration> _attributes;
        [Inspectable]
        public List<AttributeDeclaration> attributes
        {
            get => _attributes ??= new List<AttributeDeclaration>();
            set => _attributes = value;
        }

        public TypeParam()
        {
        }

        public TypeParam(Type type, string name)
        {
            this.type = type;
            this.name = name;
            Paramtype = new SystemType(type);
        }

        public TypeParam(int generic, string name)
        {
            this.generic = generic;
            this.name = name;
            usesGeneric = true;
            Paramtype = new SystemType(typeof(object));
            type = typeof(object);
        }

        public object GetDefaultValue()
        {
            if (!string.IsNullOrEmpty(defaultValueSerialized.json))
            {
                return defaultValueSerialized.Deserialize();
            }

            return defaultValue;
        }

        public void OnBeforeSerialize()
        {
            if (defaultValue != null)
            {
                defaultValueSerialized = defaultValue.Serialize();
            }

            if (_attributes != null)
            {
                attributesSerialized = _attributes.Serialize();
            }
        }

        public void OnAfterDeserialize()
        {
            defaultValue = GetDefaultValue();
            if (!string.IsNullOrEmpty(attributesSerialized.json))
            {
                _attributes = (List<AttributeDeclaration>)attributesSerialized.Deserialize();
            }
        }
    }
}