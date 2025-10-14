using System.Collections.Generic;
using Unity.VisualScripting;
using System;
using UnityEngine;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    [Inspectable]
    [RenamedFrom("Bolt.Addons.Community.Code.MemberTypeAsset")]
    public abstract class MemberTypeAsset<TFieldDeclaration, TMethodDeclaration, TConstructorDeclaration> : CodeAsset
        where TFieldDeclaration : FieldDeclaration
        where TMethodDeclaration : MethodDeclaration
        where TConstructorDeclaration : ConstructorDeclaration
    {
        [Inspectable]
        [InspectorWide]
        [SerializeField]
        public List<TConstructorDeclaration> constructors = new List<TConstructorDeclaration>();
        [Inspectable]
        [InspectorWide]
        [SerializeField]
        public List<TFieldDeclaration> variables = new List<TFieldDeclaration>();
        [Inspectable]
        [InspectorWide]
        [SerializeField]
        public List<TMethodDeclaration> methods = new List<TMethodDeclaration>();
        [Inspectable]
        [InspectorWide]
        [SerializeField]
        public List<AttributeDeclaration> attributes = new List<AttributeDeclaration>();
        [Inspectable]
        [InspectorWide]
        [SerializeField]
        public List<SystemType> interfaces = new List<SystemType>();

        [Inspectable]
        public bool inspectable = true;
        [Inspectable]
        public bool serialized = true;
        [Inspectable]
        public bool includeInSettings = true;
        [Inspectable]
        public bool definedEvent;
        [Inspectable]
        public int order;

        public override List<Type> WithFakeTypes(List<Type> types)
        {
            base.WithFakeTypes(types);
            foreach (var method in methods)
            {
                types.AddRange(method.GetFakeTypes());
            }
            return types;
        }

#if UNITY_EDITOR
        public Texture2D icon;
        public bool constructorsOpened;
        public bool fieldsOpened;
        public bool methodsOpened;
        public bool attributesOpened;
        public bool requiredInfoOpened;
        public bool overridableMembersInfoOpened;
        public bool interfacesOpened;
#endif
    }
}
