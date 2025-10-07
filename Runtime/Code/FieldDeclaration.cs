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
    public abstract class FieldDeclaration : ScriptableObject
    {
        [Inspectable]
        public Type type = typeof(int);

        [Inspectable]
        public object value;

        public string FieldName;

        public AccessModifier scope;
        public FieldModifier fieldModifier;
        public PropertyModifier propertyModifier;

        public bool isProperty;
        public bool get = true;
        public AccessModifier getterScope;
        public bool set = true;
        public AccessModifier setterScope;

        public List<AttributeDeclaration> attributes = new List<AttributeDeclaration>();

        public CodeAsset parentAsset;

        public PropertyGetterMacro getter;
        public PropertySetterMacro setter;

        [SerializeField]
        [HideInInspector]
        private string qualifiedName;

        public Action OnChanged;
#if UNITY_EDITOR
        public bool opened;
        public bool propertyOpened;
        public bool attributesOpened;
#endif

    }
}