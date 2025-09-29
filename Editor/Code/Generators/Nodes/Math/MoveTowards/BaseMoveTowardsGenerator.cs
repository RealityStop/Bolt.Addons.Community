using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public abstract class BaseMoveTowardsGenerator<T> : NodeGenerator<MoveTowards<T>>
    {
        protected abstract Type MoveTowardsClass { get; }
        public BaseMoveTowardsGenerator(Unit unit) : base(unit) { }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            data.SetExpectedType(typeof(T));
            string current = GenerateValue(Unit.current, data);
            string target = GenerateValue(Unit.target, data);
            data.RemoveExpectedType();
            data.SetExpectedType(typeof(float));
            string delta = GenerateValue(Unit.maxDelta, data);
            data.RemoveExpectedType();
            return CodeBuilder.StaticCall(Unit, MoveTowardsClass, "MoveTowards", true, current, target, delta);
        }
    }
}