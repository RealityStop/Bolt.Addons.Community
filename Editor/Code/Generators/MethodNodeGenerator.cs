using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Utility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// Abstract base class for generating nodes that require a method To Function.
    /// </summary>
    /// <typeparam name="T">The type of Unit to decorate.</typeparam>
    public abstract class MethodNodeGenerator : CountGenerator
    {
        public abstract ControlOutput OutputPort { get; }
        public abstract List<ValueOutput> OutputValues { get; }
        public abstract AccessModifier AccessModifier { get; }
        public abstract MethodModifier MethodModifier { get; }
        public abstract string Name { get; }
        public abstract Type ReturnType { get; }
        public virtual int GenericCount { get => 0; }
        public abstract List<TypeParam> Parameters { get; }
        public virtual List<AttributeDeclaration> Attributes { get; } = new List<AttributeDeclaration>();
        public virtual string MethodBody { get; }
        public ControlGenerationData Data;
        public int indent = 0;

        protected MethodNodeGenerator(Unit unit) : base(unit)
        {
        }
    }
}