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
    [NodeGenerator(typeof(ToggleValue))]
    public class ToggleValueGenerator : VariableNodeGenerator
    {
        public ToggleValueGenerator(ToggleValue unit) : base(unit)
        {
            NameSpaces = "Unity.VisualScripting.Community";
            variableName = Name;
        }
        private ToggleValue Unit => unit as ToggleValue;
        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override FieldModifier FieldModifier => FieldModifier.None;

        public override string Name => "toggleValue" + count;

        public override Type Type => typeof(ToggleValueLogic);

        public override object DefaultValue => new ToggleValueLogic() { startOn = Unit.startOn };

        public override bool HasDefaultValue => true;

        public override bool Literal => true;

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            variableName = Name;
            if (!typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType))
            {
                return CodeBuilder.Indent(indent + 1) + MakeClickableForThisUnit(CodeUtility.ToolTip("ToggleValue only works with ScriptGraphAssets, ScriptMachines or a ClassAsset that inherits MonoBehaviour", "Could not generate ToggleValue", ""));
            }

            var output = string.Empty;
            if (!data.scopeGeneratorData.TryGetValue(Unit.turnOn, out _))
            {
                data.scopeGeneratorData.Add(Unit.turnOn, true);

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

            if (input == Unit.turnOn)
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
            else if (output == Unit.value)
            {
                return Unit.CreateClickableString().Parentheses(inner => inner.Select(condition => condition.GetMember(variableName.VariableHighlight(), "isOn"), GenerateValue(Unit.onValue, data), GenerateValue(Unit.offValue, data)));
            }
            return base.GenerateValue(output, data);
        }
    }
}