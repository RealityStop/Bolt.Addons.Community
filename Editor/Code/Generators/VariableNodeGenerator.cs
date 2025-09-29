using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;
namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// Abstract base class for generating nodes that require a class-wide variable.
    /// </summary>
    /// <typeparam name="T">The type of Unit to decorate.</typeparam>
    public abstract class VariableNodeGenerator : CountGenerator
    {
        public abstract AccessModifier AccessModifier { get; }
        public abstract FieldModifier FieldModifier { get; }
        public abstract string Name { get; }
        public abstract Type Type { get; }
        public abstract object DefaultValue { get; }
        public abstract bool HasDefaultValue { get; }

        // Extra generation info
        public virtual bool Literal => false;
        public virtual bool NewLineLiteral => false;
        public virtual bool IsNew => true;

        public ControlGenerationData data;

        
        protected VariableNodeGenerator(Unit unit) : base(unit)
        {
        }
    }

    public abstract class CountGenerator : NodeGenerator
    {
        protected CountGenerator(Unit unit) : base(unit)
        {
        }
        public int count;
    }
}