using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(StopwatchUnit))]
    public sealed class StopwatchUnitGenerator : VariableNodeGenerator
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
        }

        public override IEnumerable<string> GetNamespaces()
        {
            yield return "System.Diagnostics";
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.GetVariable(Name);

            if (output == Unit.ElapsedMilliseconds)
            {
                writer.GetMember("Elapsed").GetMember("Milliseconds");
            }
            else if (output == Unit.ElapsedSeconds)
            {
                writer.GetMember("Elapsed").GetMember("TotalSeconds");
            }
            else if (output == Unit.ElapsedMinutes)
            {
                writer.GetMember("Elapsed").GetMember("TotalMinutes");
            }
            else if (output == Unit.ElapsedHours)
            {
                writer.GetMember("Elapsed").GetMember("TotalHours");
            }
            else if (output == Unit.IsRunning)
            {
                writer.GetMember("IsRunning");
            }
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input == Unit.Start)
            {
                writer.WriteIndented();
                writer.GetVariable(Name).InvokeMember(null, "Start").WriteEnd(EndWriteOptions.LineEnd);
                GenerateExitControl(Unit.Started, data, writer);
            }
            else if (input == Unit.Stop)
            {
                writer.WriteIndented();
                writer.GetVariable(Name).InvokeMember(null, "Stop").WriteEnd(EndWriteOptions.LineEnd);
                GenerateExitControl(Unit.Stopped, data, writer);
            }
            else if (input == Unit.Reset)
            {
                writer.WriteIndented();
                writer.GetVariable(Name).InvokeMember(null, "Reset").WriteEnd(EndWriteOptions.LineEnd);
            }
        }
    }
}