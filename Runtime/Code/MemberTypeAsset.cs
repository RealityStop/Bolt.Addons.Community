using System.Collections.Generic;
using Unity.VisualScripting;
using System;
using UnityEngine;

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
        public bool inspectable = true;
        [Inspectable]
        public bool serialized = true;
        [Inspectable]
        public bool includeInSettings = true;
        [Inspectable]
        public bool definedEvent;
        [Inspectable]
        public int order;

#if UNITY_EDITOR
        public Texture2D icon;
        public bool constructorsOpened;
        public bool fieldsOpened;
        public bool methodsOpened;
#endif
    }
}
