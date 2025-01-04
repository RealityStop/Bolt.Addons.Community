using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

[NodeGenerator(typeof(Timer))]
public class TimerGenerator : VariableNodeGenerator
{
    public TimerGenerator(Timer unit) : base(unit)
    {
        NameSpace = "Unity.VisualScripting.Community";
    }
    private Timer Unit => unit as Timer;
    public override AccessModifier AccessModifier => AccessModifier.Private;

    public override FieldModifier FieldModifier => FieldModifier.None;

    public override string Name => "timer" + count;

    public override Type Type => typeof(TimerLogic);

    public override object DefaultValue => new TimerLogic();

    public override bool HasDefaultValue => true;

    private bool _generatedOnTick = false;
    private bool _generatedOnCompleted = false;

    public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
    {
        variableName = Name;
        if (data.ScriptType != typeof(MonoBehaviour))
        {
            return CodeBuilder.Indent(indent + 1) + MakeSelectableForThisUnit(CodeUtility.ToolTip("Timers only work with ScriptGraphAssets or a ClassAsset that inherits MonoBehaviour",  "Could not generate Timer", ""));
        }

        var output = string.Empty;

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

        if (Unit.tick.hasValidConnection && !_generatedOnTick)
        {
            _generatedOnTick = true;
            output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit(variableName.VariableHighlight() + "." + "OnTick".VariableHighlight() + " += ") + GetAction(Unit.tick, indent, data) + MakeSelectableForThisUnit(";") + "\n";
        }

        if (Unit.completed.hasValidConnection && !_generatedOnCompleted)
        {
            _generatedOnCompleted = true;
            output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit(variableName.VariableHighlight() + "." + "OnCompleted".VariableHighlight() + " += ") + GetAction(Unit.completed, indent, data) + MakeSelectableForThisUnit(";") + "\n";
        }

        return output;
    }

    public string GetAction(ControlOutput controlOutput, int indent, ControlGenerationData data)
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
}