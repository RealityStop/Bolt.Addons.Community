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

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            writer.CallCSharpUtilityMethod("TriggerAssetCustomEvent",
                writer.Action(w => GenerateValue(Unit.asset, data, w)),
                writer.Action(w => GenerateValue(Unit.name, data, w)),
                writer.Action(w => {
                    writer.Write("new".ConstructHighlight());
                    writer.Write("object".ConstructHighlight());
                    writer.Write("[]");
                    writer.Write("{");
                    var args = Unit.args;
                    for (int i = 0; i < args.Count; i++)
                    {
                        GenerateValue(args[i], data, w);
                        if (i < args.Count - 1) writer.Write(", ");
                    }
                    writer.Write("}");
                })
            );
            writer.Write(";");
            writer.NewLine();
            GenerateExitControl(Unit.exit, data, writer);
        }
    }
}