using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using System;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using System.Reflection;

namespace Unity.VisualScripting.Community
{
    [CreateAssetMenu(menuName = "Visual Scripting/Community/Code/Class")]
    [RenamedFrom("Bolt.Addons.Community.Code.ClassAsset")]
    public class ClassAsset : MemberTypeAsset<ClassFieldDeclaration, ClassMethodDeclaration, ClassConstructorDeclaration>, IGraphRoot
    {
        [Inspectable]
        public bool scriptableObject;
        [Inspectable]
        public string fileName, menuName;
        public bool inheritsType;

        public SystemType inherits = new SystemType(null);

        [DoNotSerialize]
        public Action<Type> onValueChanged;

        [InspectorWide]
        [SerializeField]
        public List<GenericParameter> typeParameters = new List<GenericParameter>();

        [SerializeField]
        private SerializationData typeParametersSerialization;

        public IGraph childGraph => GetFirstGraph();

        public bool isSerializationRoot => true;

        public UnityEngine.Object serializedObject => this;

        public void OnAfterDeserialize()
        {
            typeParameters = (List<GenericParameter>)typeParametersSerialization.Deserialize();
        }

        public void OnBeforeSerialize()
        {
            typeParametersSerialization = typeParameters.Serialize();
        }

        public Type GetInheritedType()
        {
            if (inherits.type == null)
            {
                return null;
            }

            return ConstructType(inherits.type, typeParameters);
        }

        private Type ConstructType(Type baseType, List<GenericParameter> parameters)
        {
            if (baseType.IsGenericTypeDefinition)
            {
                try
                {
                    var genericArguments = parameters.Select(tp =>
                    {
                        if (tp.selectedType.type == null)
                        {
                            throw new ArgumentException($"Type argument for {tp.name} is null");
                        }
                        return ConstructType(tp.selectedType.type, tp.nestedParameters);
                    }).ToArray();

                    var constructedType = baseType.MakeGenericType(genericArguments);
                    return constructedType;
                }
                catch (ArgumentException ex)
                {
                    Debug.LogError($"Failed to construct generic type: {ex.Message}");
                    return baseType;
                }
            }

            return baseType;
        }

        public GraphPointer GetReference()
        {
            return GetFirstReference();
        }

        public IGraph DefaultGraph()
        {
            return new FlowGraph();
        }

        private GraphReference GetFirstReference()
        {
            var variables = this.variables.Where(variable => variable.isProperty);
            if (constructors.Count > 0 && ((constructors[0].GetReference() as GraphReference).graph as FlowGraph).units.Count > 1)
            {
                return constructors[0].GetReference() as GraphReference;
            }
            else if (variables.Count() > 0)
            {
                var first = variables.First();
                if (first.get && ((first.getter.GetReference() as GraphReference).graph as FlowGraph).units.Count > 1)
                {
                    return first.getter.GetReference() as GraphReference;
                }
                else if (first.set && ((first.setter.GetReference() as GraphReference).graph as FlowGraph).units.Count > 1)
                {
                    return first.setter.GetReference() as GraphReference;
                }
            }
            else if (methods.Count > 0 && ((methods[0].GetReference() as GraphReference).graph as FlowGraph).units.Count > 1)
            {
                return methods[0].GetReference() as GraphReference;
            }
            return null;
        }

        private IGraph GetFirstGraph()
        {
            var variables = this.variables.Where(variable => variable.isProperty);
            if (constructors.Count > 0 && (constructors[0]?.graph).units.Count > 1)
            {
                return constructors[0].graph;
            }
            else if (variables.Count() > 0)
            {
                var first = variables.First();
                if (first.get && first.getter.graph.units.Count > 1)
                {
                    return first.getter.graph;
                }
                else if (first.set && first.setter.graph.units.Count > 1)
                {
                    return first.setter.graph;
                }
            }
            else if (methods.Count > 0 && methods[0].graph.units.Count > 1)
            {
                return methods[0].graph;
            }
            return null;
        }
    }
}
