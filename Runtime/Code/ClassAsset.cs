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
    public class ClassAsset : MemberTypeAsset<ClassFieldDeclaration, ClassMethodDeclaration, ClassConstructorDeclaration>
    {
        [Inspectable]
        public bool scriptableObject;
        [Inspectable]
        public string fileName, menuName;
        public bool inheritsType;
        public ClassModifier classModifier;
        public SystemType inherits = new SystemType(null);

        [DoNotSerialize]
        public Action<Type> onValueChanged;

        [InspectorWide]
        public List<GenericParameter> typeParameters = new List<GenericParameter>();

        [SerializeField]
        private SerializationData typeParametersSerialization;

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
    }
}
