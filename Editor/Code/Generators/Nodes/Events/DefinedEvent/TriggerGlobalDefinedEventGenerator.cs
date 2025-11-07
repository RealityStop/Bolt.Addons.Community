using System;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(TriggerGlobalDefinedEvent))]
    public class TriggerGlobalDefinedEventGenerator : NodeGenerator<TriggerGlobalDefinedEvent>
    {
        public TriggerGlobalDefinedEventGenerator(Unit unit) : base(unit) { }

        private Type EventType => Unit.IsRestricted ? Unit.RestrictedEventType : Unit.EventType;

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var builder = Unit.CreateClickableString();
            builder.Indent(indent);
            void ActionCode(ClickableStringBuilder parameter)
            {
                if (Unit.IsRestricted)
                {
                    parameter.InvokeMember(
                        typeof(CSharpUtility),
                        nameof(CSharpUtility.CreateDefinedEventInstance),
                        new Type[] { EventType },
                        Unit.inputPorts.Select(port => (Action<ClickableStringBuilder>)(sb => sb.Ignore(GenerateValue(port, data)))).ToArray()
                    );
                }
                else
                {
                    parameter.Ignore(GenerateValue(Unit.inputPorts[0], data));
                }
            }
            builder.InvokeMember(typeof(DefinedEvent), nameof(DefinedEvent.TriggerGlobal), ActionCode).Clickable(";").NewLine();
            builder.Ignore(GetNextUnit(Unit.exit, data, indent));
            return builder;
        }
    }
}