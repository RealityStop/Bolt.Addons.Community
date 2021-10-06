using Unity.VisualScripting;
using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Utility;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    [Inspectable]
    [RenamedFrom("Bolt.Addons.Community.Code.MethodDeclaration")]
    public abstract class MethodDeclaration : Macro<FlowGraph>
    {
        [Inspectable]
        public string methodName;

        [Inspectable]
        [TypeFilter(Abstract = true, Classes = true, Enums = true, Generic = false, Interfaces = true,
            Nested = true, NonPublic = false, NonSerializable = true, Object = true, Obsolete = false, OpenConstructedGeneric = false, 
            Primitives = true, Public = true, Reference = true, Sealed = true, Static = false, Structs = true, Value = true)]
        public Type returnType = typeof(Libraries.CSharp.Void);

        [SerializeField]
        [HideInInspector]
        private string qualifiedReturnTypeName;

        [Inspectable]
        public ClassAsset classAsset;

        [Inspectable]
        public StructAsset structAsset;

        [Serialize][InspectorWide]
        public List<TypeParam> parameters = new List<TypeParam>();

        public AccessModifier scope = AccessModifier.Public;

        public MethodModifier modifier = MethodModifier.None;

#if UNITY_EDITOR
        public bool opened;
        public bool parametersOpened;
#endif

        public override FlowGraph DefaultGraph()
        {
            return new FlowGraph();
        }

        protected override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();

            if (!(string.IsNullOrWhiteSpace(qualifiedReturnTypeName) || string.IsNullOrEmpty(qualifiedReturnTypeName)))
            {
                returnType = Type.GetType(qualifiedReturnTypeName);
            }
        }

        protected override void OnBeforeSerialize()
        {
            base.OnBeforeSerialize();

            if (returnType == null)
            {
                qualifiedReturnTypeName = string.Empty;
                return;
            }

            qualifiedReturnTypeName = returnType.AssemblyQualifiedName;
        }
    }
}
