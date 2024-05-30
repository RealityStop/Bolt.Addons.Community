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
    public abstract class VariableNodeGenerator<T> : NodeGenerator<T> where T : Unit
    {
        public abstract AccessModifier AccessModifier { get; }
        public abstract FieldModifier FieldModifier { get; }
        public abstract string Name { get; }
        public abstract Type Type { get; }

        public int count;
        protected VariableNodeGenerator(T unit) : base(unit)
        {
        }
    }
}