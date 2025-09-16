using System;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(ToggleFlow))]
    public class ToggleFlowGenerator : VariableNodeGenerator
    {
        public ToggleFlowGenerator(ToggleFlow unit) : base(unit)
        {
            NameSpaces = "Unity.VisualScripting.Community";
            variableName = Name;
        }
        private ToggleFlow Unit => unit as ToggleFlow;
        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override FieldModifier FieldModifier => FieldModifier.None;

        public override string Name => "toggleFlow" + count;

        public override Type Type => typeof(ToggleFlowLogic);

        public override object DefaultValue => new ToggleFlowLogic() { startOn = Unit.startOn };

        public override bool HasDefaultValue => true;

        public override bool Literal => true;

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            variableName = Name;
            if (!typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType))
            {
                return CodeBuilder.Indent(indent + 1) + MakeClickableForThisUnit(CodeUtility.ToolTip("ToggleFlow only works with ScriptGraphAssets, ScriptMachines or a ClassAsset that inherits MonoBehaviour", "Could not generate ToggleFlow", ""));
            }

            var output = string.Empty;
            if (!data.scopeGeneratorData.TryGetValue(Unit.enter, out _))
            {
                data.scopeGeneratorData.Add(Unit.enter, true);

                string turnedOnCallback = Unit.turnedOn.hasValidConnection ? GetAction(Unit.turnedOn, data) : null;
                string turnedOffCallback = Unit.turnedOff.hasValidConnection ? GetAction(Unit.turnedOff, data) : null;

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

            if (input == Unit.enter)
            {
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("if ".ControlHighlight() + $"({variableName.VariableHighlight() + "." + "isOn".VariableHighlight()})") + "\n";
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("{") + "\n";
                output += GetNextUnit(Unit.exitOn, data, indent + 1);
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("}") + "\n";
                if (Unit.exitOff.hasValidConnection)
                {
                    output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("else ".ControlHighlight()) + "\n";
                    output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("{") + "\n";
                    output += GetNextUnit(Unit.exitOff, data, indent + 1);
                    output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("}") + "\n";
                }
            }
            else if (input == Unit.turnOn)
            {
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit(variableName.VariableHighlight() + ".TurnOn();") + "\n";
            }
            else if (input == Unit.turnOff)
            {
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit(variableName.VariableHighlight() + ".TurnOff();") + "\n";
            }
            else if (input == Unit.toggle)
            {
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit(variableName.VariableHighlight() + ".Toggle();") + "\n";
            }

            GenerateActionMethod(Unit.turnedOn, data, indent);
            GenerateActionMethod(Unit.turnedOff, data, indent);

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
            if (output == Unit.isOn)
            {
                return Unit.CreateClickableString().GetMember(variableName.VariableHighlight(), "isOn");
            }
            return base.GenerateValue(output, data);
        }
    }
}