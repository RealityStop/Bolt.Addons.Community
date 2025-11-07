using System;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(TriggerAssetCustomEvent))]
    public class TriggerAssetCustomEventGenerator : NodeGenerator<TriggerAssetCustomEvent>
    {
        public TriggerAssetCustomEventGenerator(Unit unit) : base(unit) { }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var hook = "hook".VariableHighlight();
            var argList = MakeClickableForThisUnit("new ".ConstructHighlight() + "object".ConstructHighlight() + "[] {") + "\n" + string.Join(MakeClickableForThisUnit(", "), Unit.args.Select(a => GenerateValue(a, data))) + MakeClickableForThisUnit("}") + "\n";
            var builder = Unit.CreateClickableString();
            builder.Indent(indent);
            builder.CallCSharpUtilityMethod(MakeClickableForThisUnit("TriggerAssetCustomEvent"),
            p => p.Ignore(GenerateValue(Unit.asset, data)),
            p => p.Ignore(GenerateValue(Unit.name, data)),
            p => p.Body(before => before.Clickable("new ".ConstructHighlight() + "object".ConstructHighlight() + "[]"), (inner, indent) => inner.Indent(indent).Ignore(string.Join(MakeClickableForThisUnit(", "), Unit.args.Select(a => GenerateValue(a, data)))), true, indent, false)).EndLine();
            builder.Ignore(GetNextUnit(Unit.exit, data, indent));
            return builder;
        }
    }
}