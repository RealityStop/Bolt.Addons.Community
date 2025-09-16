using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(InitializePoolNode))]
    public class InitializePoolNodeGenerator : LocalVariableGenerator
    {
        private InitializePoolNode Unit => unit as InitializePoolNode;
        public InitializePoolNodeGenerator(Unit unit) : base(unit) { }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return MakeClickableForThisUnit(variableName.VariableHighlight());
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var builder = Unit.CreateClickableString();
            builder.Indent(indent);
            builder.Clickable("var ".ConstructHighlight() + (variableName = data.AddLocalNameInScope("pool", typeof(ObjectPool)).VariableHighlight()));
            if (Unit.CustomParent)
                builder.Equal(true).Clickable(typeof(ObjectPool).As().CSharpName(false, true)).Dot().MethodCall("CreatePool", p1 => p1.Ignore(GenerateValue(Unit.InitialPoolSize, data)), p2 => p2.Ignore(GenerateValue(Unit.Prefab, data)), p3 => p3.Ignore(GenerateValue(Unit.parent, data))).Clickable(";");
            else
                builder.Equal(true).Clickable(typeof(ObjectPool).As().CSharpName(false, true)).Dot().MethodCall("CreatePool", p1 => p1.Ignore(GenerateValue(Unit.InitialPoolSize, data)), p2 => p2.Ignore(GenerateValue(Unit.Prefab, data))).Clickable(";");
            builder.NewLine().Ignore(GetNextUnit(Unit.Initialized, data, indent));
            return builder;
        }

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input == Unit.parent && !input.hasValidConnection && Unit.defaultValues[input.key] == null)
            {
                return MakeClickableForThisUnit("gameObject".VariableHighlight());
            }
            return base.GenerateValue(input, data);
        }
    }
}