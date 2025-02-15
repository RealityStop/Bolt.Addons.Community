using Unity.VisualScripting;
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community;

namespace Unity.VisualScripting.Community
{
    [Inspectable]
    [Serializable]
    [RenamedFrom("Bolt.Addons.Community.Code.GenericDeclaration")]
    public sealed class GenericDeclaration
    {
        [Inspectable]
        public string name;
        [Inspectable]
        public SystemType baseTypeConstraint = new SystemType() { type = typeof(int) };
        public List<SystemType> interfaceConstraints = new List<SystemType>();
        public TypeParameterConstraints typeParameterConstraints;

        public GenericDeclaration(string name, Type baseTypeConstraint)
        {
            this.name = name;
            this.baseTypeConstraint = new SystemType() { type = baseTypeConstraint ?? typeof(object) };
            interfaceConstraints = new List<SystemType>();
            typeParameterConstraints = TypeParameterConstraints.None;
        }

        public GenericDeclaration(string name) : this(name, typeof(object)) { }
        public GenericDeclaration(string name, Type baseTypeConstraint, List<Type> interfaceConstraints, TypeParameterConstraints typeParameterConstraints) : this(name, baseTypeConstraint)
        {
            foreach (Type type in interfaceConstraints)
            {
                this.interfaceConstraints.Add(new SystemType() { type = type });
            }
            this.typeParameterConstraints = typeParameterConstraints;
        }

        public GenericDeclaration(GenericParameter genericParameter) : this(genericParameter.name, genericParameter.baseTypeConstraint)
        {
            if (genericParameter.constraints != null)
            {
                foreach (Type type in genericParameter.constraints)
                {
                    if (type.IsInterface)
                        interfaceConstraints.Add(new SystemType() { type = type });
                }
            }
            typeParameterConstraints = genericParameter.typeParameterConstraints;
        }
    }
}