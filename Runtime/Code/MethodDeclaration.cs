using Unity.VisualScripting;
using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Utility;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Reflection;
using System.Linq;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    [Inspectable]
    [TypeIcon(typeof(Method))]
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

        public int genericParameterCount = 0;

        public ValueTuple<int, List<Type>> genericParameterConstraints = new ValueTuple<int, List<Type>>();
        public ValueTuple<int, List<GenericParameterAttributes>> genericParameterAttributes = new ValueTuple<int, List<GenericParameterAttributes>>();

        /// <summary>
        /// Left this to not overwrite current methodParameters
        /// and instead get the parameters from this and move it to the serializtion variable
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private string serializedParams;

        [SerializeField]
        [HideInInspector]
        private SerializationData serialization;

        [Inspectable]
        public ClassAsset classAsset;

        [Inspectable]
        public StructAsset structAsset;

        [Serialize]
        [InspectorWide]
        public List<TypeParam> parameters = new List<TypeParam>();

        [Serialize]
        public List<AttributeDeclaration> attributes = new List<AttributeDeclaration>();

        public AccessModifier scope = AccessModifier.Public;

        public MethodModifier modifier = MethodModifier.None;

        public Action OnSerialized;
#if UNITY_EDITOR
        public bool opened;
        public bool parametersOpened;
        public bool attributesOpened;
#endif
        private FunctionNode _functionNode;
        public FunctionNode functionNode
        {
            get
            {
                var expectedUnit = graph.units[0];
                if (expectedUnit is FunctionNode functionNode)
                {
                    _functionNode ??= functionNode;
                    return _functionNode;
                }
                else
                {
                    if (!graph.units.Any(unit => unit is FunctionNode))
                    {
                        throw new Exception(methodName + " on " + classAsset.name + " does not contain a function unit in the main graph!");
                    }

                    _functionNode ??= graph.units.First(unit => unit is FunctionNode) as FunctionNode;
                    return _functionNode;
                }
            }
        }
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

            foreach (var param in parameters)
            {
                param.OnAfterDeserialize();
            }

            OnSerialized?.Invoke();
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
            foreach (var param in parameters)
            {
                param.OnBeforeSerialize();
            }
        }
    }
    /// <summary>
    /// This is a empty class used for the typeIcon
    /// it does not have any functionality
    /// </summary>
    public class Method
    {
    }
}