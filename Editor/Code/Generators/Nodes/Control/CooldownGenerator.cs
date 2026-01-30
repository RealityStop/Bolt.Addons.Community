using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Cooldown))]
    public class CooldownGenerator : VariableNodeGenerator
    {
        public CooldownGenerator(Cooldown unit) : base(unit)
        {
            variableName = Name;
        }

        private Cooldown Unit => unit as Cooldown;

        public override AccessModifier AccessModifier => AccessModifier.Private;
        public override FieldModifier FieldModifier => FieldModifier.None;
        public override string Name => "cooldown" + count;
        public override Type Type => typeof(CooldownLogic);
        public override object DefaultValue => new CooldownLogic();
        public override bool HasDefaultValue => true;

        public override IEnumerable<string> GetNamespaces()
        {
            yield return "Unity.VisualScripting.Community";
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            variableName = Name;

            if (!typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType))
            {
                writer.WriteErrorDiagnostic("Cooldown only works with ScriptGraphAssets, ScriptMachines or a ClassAsset that inherits MonoBehaviour", 
                "Could not generate Cooldown", WriteOptions.IndentedNewLineAfter);
                return;
            }

            if (!data.scopeGeneratorData.TryGetValue(Unit.enter, out _))
            {
                data.scopeGeneratorData.Add(Unit.enter, true);

                writer.WriteIndented();
                writer.InvokeMember(
                    variableName.VariableHighlight(),
                    "Initialize",
                    WriteAction(Unit.exitReady, data),
                    WriteAction(Unit.exitNotReady, data),
                    WriteAction(Unit.tick, data),
                    WriteAction(Unit.becameReady, data));
                writer.WriteEnd(EndWriteOptions.LineEnd);
            }

            if (input == Unit.enter)
            {
                writer.WriteIndented();
                writer.InvokeMember(
                    variableName.VariableHighlight(),
                    "StartCooldown",
                    writer.Action(() => GenerateValue(Unit.duration, data, writer)),
                    writer.Action(() => GenerateValue(Unit.unscaledTime, data, writer)));
                writer.WriteEnd(EndWriteOptions.LineEnd);
            }
            else if (input == Unit.reset)
            {
                writer.WriteIndented();
                writer.InvokeMember(variableName.VariableHighlight(), "ResetCooldown");
                writer.WriteEnd(EndWriteOptions.LineEnd);
            }

            GenerateActionMethod(Unit.exitReady, data, writer);
            GenerateActionMethod(Unit.exitNotReady, data, writer);
            GenerateActionMethod(Unit.tick, data, writer);
            GenerateActionMethod(Unit.becameReady, data, writer);
        }

        private Action<CodeWriter> WriteAction(ControlOutput port, ControlGenerationData data)
        {
            if (!port.hasValidConnection)
                return w => w.Write("null".ConstructHighlight());

            return w => w.Write(GetAction(port));
        }

        private void GenerateActionMethod(ControlOutput port, ControlGenerationData data, CodeWriter writer)
        {
            if (!port.hasValidConnection || data.scopeGeneratorData.TryGetValue(port, out _))
                return;

            data.scopeGeneratorData.Add(port, true);

            writer.WriteIndented("void ".ConstructHighlight());
            writer.Write(GetAction(port));
            writer.Parentheses();
            writer.NewLine();

            writer.WriteLine("{");

            using (writer.IndentedScope(data))
            {
                GenerateChildControl(port, data, writer);
            }

            writer.WriteLine("}");
        }

        private string GetAction(ControlOutput controlOutput)
        {
            return variableName.Capitalize().First().Letter() + "_" + controlOutput.key.Capitalize().First().Letter();
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (output == Unit.remainingSeconds)
            {
                writer.GetMember(variableName.VariableHighlight(), "RemainingTime");
                return;
            }

            if (output == Unit.remainingRatio)
            {
                writer.GetMember(variableName.VariableHighlight(), "RemainingPercentage");
                return;
            }
        }
    }
}