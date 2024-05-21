using Unity.VisualScripting;
using System;
using UnityEngine;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    [Inspectable]
    [RenamedFrom("Bolt.Addons.Community.Code.FieldDeclaration")]
    public abstract class FieldDeclaration : ScriptableObject, ISerializationCallbackReceiver
    {
        [Inspectable]
        public Type type = typeof(int);
        [Inspectable]
        public bool inspectable = true;
        [Inspectable]
        public bool serialized = true;

        [Inspectable]
        public object value;

        public string FieldName;

        public AccessModifier scope;
        public FieldModifier fieldModifier;
        public PropertyModifier propertyModifier;

        public bool isProperty;
        public bool get = true;
        public bool set = true;

        public List<AttributeDeclaration> attributes = new List<AttributeDeclaration>();

        public ClassAsset classAsset;
        public StructAsset structAsset;

        public PropertyGetterMacro getter;
        public PropertySetterMacro setter;

        [SerializeField]
        [HideInInspector]
        private string qualifiedName;

        [SerializeField]
        [HideInInspector]
        private string serializedValue;

        [SerializeField]
        [HideInInspector]
        private string serializedValueType;

#if UNITY_EDITOR
        public bool opened;
        public bool propertyOpened;
        public bool attributesOpened;
#endif

        public void OnAfterDeserialize()
        {
            if (!(string.IsNullOrWhiteSpace(qualifiedName) || string.IsNullOrEmpty(qualifiedName)))
            {
                type = Type.GetType(qualifiedName);
            }

            if (!string.IsNullOrEmpty(serializedValue) && !string.IsNullOrEmpty(serializedValueType) && Type.GetType(serializedValueType) != typeof(Delegate))
            {
                value = JsonConvert.DeserializeObject(serializedValue, Type.GetType(serializedValueType));
            }
        }

        public void OnBeforeSerialize()
        {
            if (type == null)
            {
                qualifiedName = string.Empty;
                return;
            }

            qualifiedName = type.AssemblyQualifiedName;

            if (value == null)
            {
                serializedValue = string.Empty;
                return;
            }
        }
    }
}

[Serializable]
public class DelegateInfo
{
    public string MethodName { get; set; }
    public string TypeName { get; set; }
}