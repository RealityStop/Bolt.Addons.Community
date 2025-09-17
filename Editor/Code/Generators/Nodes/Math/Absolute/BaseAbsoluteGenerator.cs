using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public abstract class BaseAbsoluteGenerator<T> : NodeGenerator<Absolute<T>>
    {
        public BaseAbsoluteGenerator(Unit unit) : base(unit) { }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            data.SetExpectedType(typeof(T));
            var value = GenerateValue(Unit.input, data);
            data.RemoveExpectedType();
            return CodeBuilder.CallCSharpUtilityMethod(Unit, MakeClickableForThisUnit("Absolute"), value);
        }
    }
}