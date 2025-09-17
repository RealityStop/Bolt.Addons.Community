using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public abstract class BaseMaximumGenerator<T> : NodeGenerator<Maximum<T>>
    {
        public BaseMaximumGenerator(Unit unit) : base(unit) { }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            List<string> values = new();
            foreach (var input in Unit.multiInputs)
            {
                data.SetExpectedType(typeof(T));
                values.Add(GenerateValue(input, data));
                data.RemoveExpectedType();
            }
            return CodeBuilder.CallCSharpUtilityMethod(Unit, MakeClickableForThisUnit("CalculateMax"), string.Join(MakeClickableForThisUnit(", "), values));
        }
    }
}