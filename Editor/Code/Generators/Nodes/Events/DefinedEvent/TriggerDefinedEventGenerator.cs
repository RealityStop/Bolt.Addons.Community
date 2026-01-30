using System;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(TriggerDefinedEvent))]
    public class TriggerDefinedEventGenerator : NodeGenerator<TriggerDefinedEvent>
    {
        public TriggerDefinedEventGenerator(Unit unit) : base(unit) { }

        private Type EventType => Unit.IsRestricted ? Unit.RestrictedEventType : Unit.EventType;


        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            writer.WriteIndented(typeof(DefinedEvent).As().CSharpName(false, true));
            writer.Write(".");
            writer.Write("Trigger");
            writer.Write("(");
            GenerateValue(Unit.EventTarget, data, writer);
            writer.Write(", ");
            if (Unit.IsRestricted)
            {
                writer.CallCSharpUtilityGenericMethod("CreateDefinedEventInstance", new CodeWriter.TypeParameter[] { EventType },
                    Unit.inputPorts.Select(port => (CodeWriter.MethodParameter)writer.Action(w => GenerateValue(port, data, w))).ToArray()
                );
            }
            else
            {
                GenerateValue(Unit.inputPorts[0], data, writer);
            }
            writer.Write(")");
            writer.Write(";");
            writer.NewLine();
            GenerateExitControl(Unit.exit, data, writer);
        }
    }
}