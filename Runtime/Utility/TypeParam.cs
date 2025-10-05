using Unity.VisualScripting.Community.Libraries.CSharp;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Unity.VisualScripting.Community.Utility
{
    [Serializable]
    [Inspectable]
    [RenamedFrom("Bolt.Addons.Community.Utility.TypeParam")]
    public sealed class TypeParam : ISerializationCallbackReceiver
    {
        [SerializeField]
        [Inspectable]
        [FullSerializer.fsProperty(Converter = typeof(FakeGenericParameterTypeConverter))]
        public Type type = typeof(object);

        [SerializeField]
        [UsedImplicitly]
        public SystemType Paramtype = new SystemType(typeof(object));

        [SerializeField]
        [Inspectable]
        public string name;

        [InspectableIf("showDefault")]
        public bool hasDefault;

        [InspectableIf("showInitalizer")]
        [RenamedFrom("useInCall")]
        public bool useInInitializer;

        public bool usesGeneric { get; }

        public int generic;

        [SerializeField]
        [Obsolete("Use modifier instead")]
        public bool isParamsParameter;

        [Serialize]
        [SerializeField]
        [InspectorToggleLeft]
        public object defaultValue;
#if VISUAL_SCRIPTING_1_7
        public SerializableType typeHandle;
#endif
        [RenamedFrom("showCall")]
        public bool showInitalizer = false;
        public bool showDefault = false;
        public bool supportsAttributes = true;

#if UNITY_EDITOR
        public bool attributesOpened;
        public bool opened;
#endif

        [SerializeField]
        private SerializationData defaultValueSerialized;
        [SerializeField]
        private SerializationData attributesSerialized;

        [Inspectable]
        [DoNotSerialize]
        public ParameterModifier modifier = ParameterModifier.None;
        [SerializeField]
        [Serialize]
        private string modifierSerialization;
        [NonSerialized]
        private List<AttributeDeclaration> _attributes;
        [InspectableIf(nameof(supportsAttributes))]
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

            if (modifier == ParameterModifier.None)
                modifierSerialization = "";
            else
                modifierSerialization = modifier.GetEnumString(ParameterModifier.None);
        }

        public void OnAfterDeserialize()
        {
            defaultValue = GetDefaultValue();
            if (!string.IsNullOrEmpty(attributesSerialized.json))
            {
                _attributes = (List<AttributeDeclaration>)attributesSerialized.Deserialize();
            }
            if (!string.IsNullOrEmpty(modifierSerialization))
                DeserializeModifer();
            else
                modifier = ParameterModifier.None;
        }

        private void DeserializeModifer()
        {
            if (modifierSerialization.Contains("In"))
                modifier |= ParameterModifier.In;
            if (modifierSerialization.Contains("Out"))
                modifier |= ParameterModifier.Out;
            if (modifierSerialization.Contains("Ref"))
                modifier |= ParameterModifier.Ref;
            if (modifierSerialization.Contains("Params"))
                modifier |= ParameterModifier.Params;
            if (modifierSerialization.Contains("This"))
                modifier |= ParameterModifier.This;
        }
    }
}