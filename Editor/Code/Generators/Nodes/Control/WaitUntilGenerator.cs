using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(WaitUntilUnit))]
    public class WaitUntilGenerator : NodeGenerator<WaitUntilUnit>
    {
        public WaitUntilGenerator(Unit unit) : base(unit) { }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;
            data.SetExpectedType(typeof(bool));
            var condition = GenerateValue(Unit.condition, data);
            data.RemoveExpectedType();
            output += CodeBuilder.Indent(indent) + typeof(WaitUntil).Create(condition.ExpressionLambda(Unit), true, true, Unit).YieldReturn(true, Unit) + "\n";
            output += GetNextUnit(Unit.exit, data, indent);
            return output;
        }
    }
}
