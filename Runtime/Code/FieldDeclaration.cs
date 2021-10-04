using Unity.VisualScripting;
using System;
using UnityEngine;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;

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
        } 

        public void OnBeforeSerialize()
        {
            if (type == null)
            {
                qualifiedName = string.Empty;
                return;
            }

            qualifiedName = type.AssemblyQualifiedName;
        }
    }
}
