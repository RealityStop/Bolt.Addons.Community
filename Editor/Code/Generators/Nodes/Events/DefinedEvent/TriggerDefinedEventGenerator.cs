using System;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(TriggerDefinedEvent))]
    public class TriggerDefinedEventGenerator : NodeGenerator<TriggerDefinedEvent>
    {
        public TriggerDefinedEventGenerator(Unit unit) : base(unit) { }

        private Type EventType => Unit.IsRestricted ? Unit.RestrictedEventType : Unit.EventType;

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var builder = Unit.CreateClickableString();
            builder.Indent(indent);
            System.Action<ClickableStringBuilder> action = (p2) =>
            {
                if (Unit.IsRestricted)
                {
                    p2.InvokeMember(
                        typeof(CSharpUtility),
                        nameof(CSharpUtility.CreateDefinedEventInstance),
                        new Type[] { EventType },
                        Unit.inputPorts.Select(port =>(Action<ClickableStringBuilder>)(sb => sb.Ignore(GenerateValue(port, data)))).ToArray()
                    );
                }
                else
                {
                    p2.Ignore(GenerateValue(Unit.inputPorts[0], data));
                }
            };
            builder.InvokeMember(typeof(DefinedEvent), nameof(DefinedEvent.Trigger), p1 => p1.Ignore(GenerateValue(Unit.EventTarget, data)), action).Clickable(";").NewLine();
            builder.Ignore(GetNextUnit(Unit.exit, data, indent));
            return builder;
        }
    }
}