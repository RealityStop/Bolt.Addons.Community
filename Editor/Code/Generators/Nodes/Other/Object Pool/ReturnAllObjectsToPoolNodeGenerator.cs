using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(ReturnAllObjectsToPoolNode))]
    public class ReturnAllObjectsToPoolNodeGenerator : NodeGenerator<ReturnAllObjectsToPoolNode>
    {
        public ReturnAllObjectsToPoolNodeGenerator(Unit unit) : base(unit) { }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var builder = Unit.CreateClickableString();
            builder.Indent(indent);
            builder.Clickable(typeof(ObjectPool).As().CSharpName(false, true)).Dot().MethodCall("ReturnAllObjects", p1 => p1.Ignore(GenerateValue(Unit.Pool, data))).Clickable(";");
            builder.NewLine().Ignore(GetNextUnit(Unit.Exit, data, indent));
            return builder;
        }

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input == Unit.Pool && !input.hasValidConnection && Unit.defaultValues[input.key] == null)
            {
                return MakeClickableForThisUnit("gameObject".VariableHighlight() + ".GetComponent<" + typeof(ObjectPool).As().CSharpName(false, true) + ">()");
            }
            return base.GenerateValue(input, data);
        }
    }
}