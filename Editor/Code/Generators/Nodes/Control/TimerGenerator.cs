using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community 
{
    [NodeGenerator(typeof(Timer))]
    public class TimerGenerator : UpdateVariableNodeGenerator
    {
        public TimerGenerator(Timer unit) : base(unit)
        {
            NameSpaces = "Unity.VisualScripting.Community";
        }
        private Timer Unit => unit as Timer;
        public override AccessModifier AccessModifier => AccessModifier.Private;
    
        public override FieldModifier FieldModifier => FieldModifier.None;
    
        public override string Name => "timer" + count;
    
        public override Type Type => typeof(TimerLogic);
    
        public override object DefaultValue => new TimerLogic();
    
        public override bool HasDefaultValue => true;
<<<<<<< Updated upstream
<<<<<<< Updated upstream
    
        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            variableName = Name;
            if(!typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType))
            {
                return CodeBuilder.Indent(indent + 1) + MakeSelectableForThisUnit(CodeUtility.ToolTip("Timers only works with ScriptGraphAssets, ScriptMachines or a ClassAsset that inherits MonoBehaviour",  "Could not generate Timer", ""));
            }
    
            var output = string.Empty;
    
=======

        public override bool Literal => false;

        protected override string GenerateCode(ControlInput input, ControlGenerationData data, int indent)
        {
            variableName = Name;
            var output = string.Empty;
=======

        public override bool Literal => false;

        protected override string GenerateCode(ControlInput input, ControlGenerationData data, int indent)
        {
            variableName = Name;
            var output = string.Empty;
>>>>>>> Stashed changes
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

>>>>>>> Stashed changes
            if (input == Unit.start)
            {
                var action = GetAction(Unit.started, indent, data);
                output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit(variableName.VariableHighlight() + ".StartTimer(") + GenerateValue(Unit.duration, data) + MakeSelectableForThisUnit(", ") + GenerateValue(Unit.unscaledTime, data) + (!string.IsNullOrEmpty(action) ? MakeSelectableForThisUnit(", ") + action : "") + MakeSelectableForThisUnit(");") + "\n";
            }
            else if (input == Unit.pause)
            {
                output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit(variableName.VariableHighlight() + ".PauseTimer();") + "\n";
            }
            else if (input == Unit.resume)
            {
                output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit(variableName.VariableHighlight() + ".ResumeTimer();") + "\n";
            }
            else if (input == Unit.toggle)
            {
                output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit(variableName.VariableHighlight() + ".ToggleTimer();") + "\n";
            }
    
            if (Unit.tick.hasValidConnection && !data.generatorData.TryGetValue(Unit.tick, out var tickGenerated))
            {
<<<<<<< Updated upstream
<<<<<<< Updated upstream
                data.generatorData.Add(Unit.tick, true);
                output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit(variableName.VariableHighlight() + "." + "OnTick".VariableHighlight() + " += ") + GetAction(Unit.tick, indent, data) + MakeSelectableForThisUnit(";") + "\n";
=======
=======
>>>>>>> Stashed changes
                if (port.hasValidConnection && !data.scopeGeneratorData.TryGetValue(port, out _))
                {
                    data.scopeGeneratorData.Add(port, true);
                    output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("void ".ConstructHighlight()) + GetAction(port, data) + MakeClickableForThisUnit("()") + "\n";
                    output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("{") + "\n";
                    output += GetNextUnit(port, data, indent + 1);
                    output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("}") + "\n";
                }
>>>>>>> Stashed changes
            }
    
            if (Unit.completed.hasValidConnection && !data.generatorData.TryGetValue(Unit.completed, out var completedGenerated))
            {
                data.generatorData.Add(Unit.completed, true);
                output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit(variableName.VariableHighlight() + "." + "OnCompleted".VariableHighlight() + " += ") + GetAction(Unit.completed, indent, data) + MakeSelectableForThisUnit(";") + "\n";
            }
    
            return output;
        }
    
        private string GetAction(ControlOutput controlOutput, int indent, ControlGenerationData data)
        {
            if (!controlOutput.hasValidConnection)
                return "";
            var output = "";
            var _data = new ControlGenerationData(data);
            output += MakeSelectableForThisUnit("() =>") + "\n";
            output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit("{") + "\n";
            _data.returns = typeof(void);
            output += GetNextUnit(controlOutput, _data, indent + 1);
            output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit("}");
            return output;
        }
    
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (output == Unit.elapsedSeconds)
            {
                return MakeSelectableForThisUnit(variableName.VariableHighlight() + "." + "Elapsed".VariableHighlight());
            }
            else if (output == Unit.elapsedRatio)
            {
                return MakeSelectableForThisUnit(variableName.VariableHighlight() + "." + "ElapsedPercentage".VariableHighlight());
            }
            else if (output == Unit.remainingSeconds)
            {
                return MakeSelectableForThisUnit(variableName.VariableHighlight() + "." + "Remaining".VariableHighlight());
            }
            else if (output == Unit.remainingRatio)
            {
                return MakeSelectableForThisUnit(variableName.VariableHighlight() + "." + "RemainingPercentage".VariableHighlight());
            }
            return base.GenerateValue(output, data);
        }
<<<<<<< Updated upstream
<<<<<<< Updated upstream
    } 
=======
=======
>>>>>>> Stashed changes

        public override string GenerateUpdateCode(ControlGenerationData data, int indent)
        {
            return CodeBuilder.Indent(indent) + MakeClickableForThisUnit(variableName.VariableHighlight() + ".Update();");
        }
    }
>>>>>>> Stashed changes
}