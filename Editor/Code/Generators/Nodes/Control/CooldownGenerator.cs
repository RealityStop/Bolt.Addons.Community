using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Cooldown))]
    public class CooldownGenerator : VariableNodeGenerator
    {
        public CooldownGenerator(Cooldown unit) : base(unit)
        {
            NameSpaces = "Unity.VisualScripting.Community";
            variableName = Name;
        }

        private Cooldown Unit => unit as Cooldown;

        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override FieldModifier FieldModifier => FieldModifier.None;

        public override string Name => "cooldown" + count;

        public override Type Type => typeof(CooldownLogic);

        public override object DefaultValue => new CooldownLogic();

        public override bool HasDefaultValue => true;

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            variableName = Name;
            if (!typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType))
            {
                return CodeBuilder.Indent(indent + 1) + MakeClickableForThisUnit(CodeUtility.ToolTip("Cooldown only works with ScriptGraphAssets, ScriptMachines or a ClassAsset that inherits MonoBehaviour", "Could not generate Cooldown", ""));
            }

            var output = string.Empty;
            if (!data.scopeGeneratorData.TryGetValue(Unit.enter, out _))
            {
                data.scopeGeneratorData.Add(Unit.enter, true);

                string readyCallback = Unit.exitReady.hasValidConnection ? GetAction(Unit.exitReady, data) : null;
                string notReadyCallback = Unit.exitNotReady.hasValidConnection ? GetAction(Unit.exitNotReady, data) : null;
                string onTickCallback = Unit.tick.hasValidConnection ? GetAction(Unit.tick, data) : null;
                string onCompleteCallback = Unit.becameReady.hasValidConnection ? GetAction(Unit.becameReady, data) : null;

                var parameters = new[]
                {
                    readyCallback ?? MakeClickableForThisUnit("null".ConstructHighlight()),
                    notReadyCallback ?? MakeClickableForThisUnit("null".ConstructHighlight()),
                    onTickCallback ?? MakeClickableForThisUnit("null".ConstructHighlight()),
                    onCompleteCallback ?? MakeClickableForThisUnit("null".ConstructHighlight())
                };

                string paramList = string.Join(MakeClickableForThisUnit(", "), parameters);

                output += CodeBuilder.Indent(indent)
                    + MakeClickableForThisUnit(variableName.VariableHighlight() + ".Initialize(")
                    + paramList
                    + MakeClickableForThisUnit(");")
                    + "\n";
            }

            if (input == Unit.enter)
            {
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit(variableName.VariableHighlight() + ".StartCooldown(") + GenerateValue(Unit.duration, data) + MakeClickableForThisUnit(", ") + GenerateValue(Unit.unscaledTime, data) + MakeClickableForThisUnit(");") + "\n";
            }
            else if (input == Unit.reset)
            {
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit(variableName.VariableHighlight() + ".ResetCooldown();") + "\n";
            }

            GenerateActionMethod(Unit.exitReady, data, indent);
            GenerateActionMethod(Unit.exitNotReady, data, indent);
            GenerateActionMethod(Unit.tick, data, indent);
            GenerateActionMethod(Unit.becameReady, data, indent);

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
            if (output == Unit.remainingSeconds)
            {
                return MakeClickableForThisUnit(variableName.VariableHighlight() + "." + "RemainingTime".VariableHighlight());
            }
            else if (output == Unit.remainingRatio)
            {
                return MakeClickableForThisUnit(variableName.VariableHighlight() + "." + "RemainingPercentage".VariableHighlight());
            }
            return base.GenerateValue(output, data);
        }
    }
}