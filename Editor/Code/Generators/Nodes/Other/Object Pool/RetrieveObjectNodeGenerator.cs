using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(RetrieveObjectNode))]
    public class RetrieveObjectNodeGenerator : LocalVariableGenerator
    {
        private RetrieveObjectNode Unit => unit as RetrieveObjectNode;
        public RetrieveObjectNodeGenerator(Unit unit) : base(unit) { }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return MakeClickableForThisUnit(variableName.VariableHighlight());
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var builder = Unit.CreateClickableString();
            builder.Indent(indent);
            builder.Clickable("var ".ConstructHighlight() + (variableName = data.AddLocalNameInScope("poolObject", typeof(PoolObject)).VariableHighlight()));
            builder.Equal(true).Ignore(GenerateValue(Unit.Pool, data)).Dot().MethodCall("RetrieveObjectFromPool").Clickable(";");
            builder.NewLine().Ignore(GetNextUnit(Unit.Retrieved, data, indent));
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