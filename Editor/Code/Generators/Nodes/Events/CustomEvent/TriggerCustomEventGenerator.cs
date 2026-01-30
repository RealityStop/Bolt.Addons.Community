using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(TriggerCustomEvent))]
    public sealed class TriggerCustomEventGenerator : NodeGenerator<TriggerCustomEvent>
    {
        public TriggerCustomEventGenerator(Unit unit) : base(unit)
        {
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            writer.WriteIndented(typeof(CustomEvent).As().CSharpName(false, true));
            writer.Write(".");
            writer.Write("Trigger");
            writer.Write("(");
            GenerateValue(Unit.target, data, writer);
            writer.Write(", ");
            GenerateValue(Unit.name, data, writer);
            if (Unit.argumentCount > 0)
            {
                writer.Write(", ");
                var args = Unit.arguments;
                for (int i = 0; i < args.Count; i++)
                {
                    GenerateValue(args[i], data, writer);
                    if (i < args.Count - 1) writer.Write(", ");
                }
            }
            writer.Write(")");
            writer.Write(";");
            writer.NewLine();
            GenerateExitControl(Unit.exit, data, writer);
        }

        protected override void GenerateValueInternal(ValueInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input == Unit.target)
            {
                if (!Unit.target.hasValidConnection && Unit.defaultValues[input.key] == null)
                {
                    writer.GetVariable("gameObject");
                    return;
                }
                else
                {
                    var sourceType = GetSourceType(Unit.target, data, writer);
                    var sourceIsComponent = typeof(Component).IsStrictlyAssignableFrom(sourceType);

                    base.GenerateValueInternal(Unit.target, data, writer);

                    if (sourceIsComponent)
                    {
                        writer.WriteConvertTo(typeof(GameObject), true);
                    }
                    return;
                }
            }
            base.GenerateValueInternal(input, data, writer);
        }
    }
}