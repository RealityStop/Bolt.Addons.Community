using Unity.VisualScripting;
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;

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
        public SystemType type = new SystemType() { type = typeof(int) };
        [Inspectable]
        public List<Constraint> constraints = new List<Constraint>();
    }
}

namespace Unity.VisualScripting.Community
{
    [Inspectable]
    [Serializable]
    public sealed class Constraint
    {
        [Inspectable]
        public SystemType type = new SystemType(typeof(object));

        [Inspectable]
        public List<SystemType> interfaces = new List<SystemType>();

        [Inspectable]
        public ConstraintType constraintType;

        public Constraint()
        {
            constraintType = ConstraintType.BaseType;
        }
    }

    public enum ConstraintType
    {
        BaseType,      // The base type constraint (e.g., a specific class like MyClass)
        Interface,     // An interface constraint (e.g., IMyInterface)
        Struct,        // The 'struct' constraint, ensuring the type is a value type
        Class,         // The 'class' constraint, ensuring the type is a reference type
        DefaultConstructor // The "new()" constraint, ensuring the type has a parameterless constructor
    }

}
