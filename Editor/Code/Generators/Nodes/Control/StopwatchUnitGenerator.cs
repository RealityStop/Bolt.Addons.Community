using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Diagnostics;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(StopwatchUnit))]
    public class StopwatchUnitGenerator : VariableNodeGenerator
    {
        private StopwatchUnit Unit => unit as StopwatchUnit;
        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override FieldModifier FieldModifier => FieldModifier.None;

        public override string Name => "stopwatch" + count;

        public override Type Type => typeof(Stopwatch);

        public override object DefaultValue => new Stopwatch();

        public override bool HasDefaultValue => true;

        public StopwatchUnitGenerator(Unit unit) : base(unit)
        {
            NameSpaces = "System.Diagnostics";
        }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            var builder = Unit.CreateClickableString();
            if (output == Unit.ElapsedMilliseconds)
            {
                builder.GetMember(Name.VariableHighlight(), "Elapsed").GetMember("Milliseconds");
            }
            else if (output == Unit.ElapsedSeconds)
            {
                builder.GetMember(Name.VariableHighlight(), "Elapsed").GetMember("TotalSeconds");
            }
            else if (output == Unit.ElapsedMinutes)
            {
                builder.GetMember(Name.VariableHighlight(), "Elapsed").GetMember("TotalMinutes");
            }
            else if (output == Unit.ElapsedHours)
            {
                builder.GetMember(Name.VariableHighlight(), "Elapsed").GetMember("TotalHours");
            }
            else if (output == Unit.IsRunning)
            {
                builder.GetMember(Name.VariableHighlight(), "IsRunning");
            }
            return builder;
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var builder = Unit.CreateClickableString();
            if (input == Unit.Start)
            {
                builder.InvokeMember(Name.VariableHighlight(), "Start", Array.Empty<string>()).EndLine();
                builder.Ignore(GetNextUnit(Unit.Started, data, indent));
            }
            else if (input == Unit.Stop)
            {
                builder.InvokeMember(Name.VariableHighlight(), "Stop", Array.Empty<string>()).EndLine();
                builder.Ignore(GetNextUnit(Unit.Stopped, data, indent));
            }
            return builder;
        }
    }
}