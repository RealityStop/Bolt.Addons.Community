using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Timer))]
    public class TimerGenerator : VariableNodeGenerator
    {
        public TimerGenerator(Timer unit) : base(unit)
        {
            NameSpaces = "Unity.VisualScripting.Community";
            variableName = Name;
        }
        private Timer Unit => unit as Timer;
        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override FieldModifier FieldModifier => FieldModifier.None;

        public override string Name => "timer" + count;

        public override Type Type => typeof(TimerLogic);

        public override object DefaultValue => new TimerLogic();

        public override bool HasDefaultValue => true;

        public override bool Literal => false;

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            variableName = Name;
            if (!typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType))
            {
                return CodeBuilder.Indent(indent + 1) + MakeClickableForThisUnit(CodeUtility.ErrorTooltip("Timer only works with ScriptGraphAssets, ScriptMachines or a ClassAsset that inherits MonoBehaviour", "Could not generate Timer", ""));
            }

            var output = string.Empty;
            if (Unit.start.hasValidConnection && !data.scopeGeneratorData.TryGetValue(Unit.start, out _))
            {
                data.scopeGeneratorData.Add(Unit.start, true);
                string turnedOnCallback = Unit.completed.hasValidConnection ? GetAction(Unit.completed, data) : null;
                string turnedOffCallback = Unit.tick.hasValidConnection ? GetAction(Unit.tick, data) : null;

                var parameters = new List<string>();
                if (turnedOnCallback != null)
                    parameters.Add(turnedOnCallback);
                else if (turnedOffCallback != null)
                    parameters.Add(MakeClickableForThisUnit("null".ConstructHighlight()));

                if (turnedOffCallback != null)
                    parameters.Add(turnedOffCallback);

                string paramList = string.Join(MakeClickableForThisUnit(", "), parameters);

                output += CodeBuilder.Indent(indent)
                    + MakeClickableForThisUnit(variableName.VariableHighlight() + ".Initialize(")
                    + paramList
                    + MakeClickableForThisUnit(");") + "\n";
            }

            if (input == Unit.start)
            {
                var action = GetAction(Unit.started, data);
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit(variableName.VariableHighlight() + ".StartTimer(") + GenerateValue(Unit.duration, data) + MakeClickableForThisUnit(", ") + GenerateValue(Unit.unscaledTime, data) + (!string.IsNullOrEmpty(action) ? MakeClickableForThisUnit(", ") + action : "") + MakeClickableForThisUnit(");") + "\n";
            }
            else if (input == Unit.pause)
            {
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit(variableName.VariableHighlight() + ".PauseTimer();") + "\n";
            }
            else if (input == Unit.resume)
            {
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit(variableName.VariableHighlight() + ".ResumeTimer();") + "\n";
            }
            else if (input == Unit.toggle)
            {
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit(variableName.VariableHighlight() + ".ToggleTimer();") + "\n";
            }

            GenerateActionMethod(Unit.started, data, indent);
            GenerateActionMethod(Unit.tick, data, indent);
            GenerateActionMethod(Unit.completed, data, indent);

            void GenerateActionMethod(ControlOutput port, ControlGenerationData data, int indent)
            {
                if (port.hasValidConnection && !data.scopeGeneratorData.TryGetValue(port, out _))
                {
                    data.scopeGeneratorData.Add(port, true);
                    output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("void ".ConstructHighlight()) + GetAction(port, data) + MakeClickableForThisUnit("()") + "\n";
                    output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("{") + "\n";
                    output += GetNextUnit(port, data, indent + 1);
                    output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("}") + "\n";
                }
            }

            return output;
        }

        private string GetAction(ControlOutput controlOutput, ControlGenerationData data)
        {
            if (!controlOutput.hasValidConnection)
                return "";
            var output = "";
            output += MakeClickableForThisUnit(variableName.Capitalize().First().Letter() + "_" + controlOutput.key.Capitalize().First().Letter());
            return output;
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (output == Unit.elapsedSeconds)
            {
                return MakeClickableForThisUnit(variableName.VariableHighlight() + "." + "Elapsed".VariableHighlight());
            }
            else if (output == Unit.elapsedRatio)
            {
                return MakeClickableForThisUnit(variableName.VariableHighlight() + "." + "ElapsedPercentage".VariableHighlight());
            }
            else if (output == Unit.remainingSeconds)
            {
                return MakeClickableForThisUnit(variableName.VariableHighlight() + "." + "Remaining".VariableHighlight());
            }
            else if (output == Unit.remainingRatio)
            {
                return MakeClickableForThisUnit(variableName.VariableHighlight() + "." + "RemainingPercentage".VariableHighlight());
            }
            return base.GenerateValue(output, data);
        }
    }
}