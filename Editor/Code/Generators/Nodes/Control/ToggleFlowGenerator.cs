using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(ToggleFlow))]
    public class ToggleFlowGenerator : VariableNodeGenerator
    {
        public ToggleFlowGenerator(ToggleFlow unit) : base(unit)
        {
            variableName = Name;
        }

        public override IEnumerable<string> GetNamespaces()
        {
            yield return "Unity.VisualScripting.Community";
        }

        private ToggleFlow Unit => unit as ToggleFlow;
        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override FieldModifier FieldModifier => FieldModifier.None;

        public override string Name => "toggleFlow" + count;

        public override Type Type => typeof(ToggleFlowLogic);

        public override object DefaultValue => new ToggleFlowLogic() { startOn = Unit.startOn };

        public override bool HasDefaultValue => true;

        public override bool Literal => true;

        private string GetAction(ControlOutput controlOutput)
        {
            if (!controlOutput.hasValidConnection)
                return "";
            var output = "";
            output += variableName.Capitalize().First().Letter() + "_" + controlOutput.key.Capitalize().First().Letter();
            return output;
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (output == Unit.isOn)
            {
                writer.GetMember(variableName.VariableHighlight(), "isOn");
            }
            else
                base.GenerateValueInternal(output, data, writer);
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            variableName = Name;
            if (!typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType))
            {
                writer.WriteErrorDiagnostic("ToggleFlow only works with ScriptGraphAssets, ScriptMachines or a ClassAsset that inherits MonoBehaviour", "Could not generate ToggleFlow", WriteOptions.IndentedNewLineAfter);
                return;
            }

            if (!data.scopeGeneratorData.TryGetValue(Unit.enter, out _))
            {
                data.scopeGeneratorData.Add(Unit.enter, true);

                string turnedOnCallback = Unit.turnedOn.hasValidConnection ? GetAction(Unit.turnedOn) : null;
                string turnedOffCallback = Unit.turnedOff.hasValidConnection ? GetAction(Unit.turnedOff) : null;

                writer.WriteIndented(variableName.VariableHighlight());
                writer.Write(".");
                writer.Write("Initialize");
                writer.Write("(");

                if (turnedOnCallback != null)
                    writer.Write(turnedOnCallback);
                else if (turnedOffCallback != null)
                    writer.Write("null".ConstructHighlight());

                if (turnedOffCallback != null)
                {
                    writer.Write(", ");
                    writer.Write(turnedOffCallback);
                }

                writer.Write(")");
                writer.Write(";");
                writer.NewLine();
            }

            if (input == Unit.enter)
            {
                writer.WriteIndented("if".ControlHighlight());
                writer.Write(" (");
                writer.Write(variableName.VariableHighlight());
                writer.Write(".");
                writer.Write("isOn".VariableHighlight());
                writer.Write(")");
                writer.NewLine();
                writer.WriteLine("{");

                using (writer.IndentedScope(data))
                {
                    GenerateChildControl(Unit.exitOn, data, writer);
                }

                writer.WriteIndented("}");

                if (Unit.exitOff.hasValidConnection)
                {
                    writer.NewLine();
                    writer.WriteIndented("else".ControlHighlight());
                    writer.NewLine();
                    writer.WriteLine("{");

                    using (writer.IndentedScope(data))
                    {
                        GenerateChildControl(Unit.exitOff, data, writer);
                    }

                    writer.WriteIndented("}");
                }
                writer.NewLine();
            }
            else if (input == Unit.turnOn)
            {
                writer.WriteIndented(variableName.VariableHighlight());
                writer.Write(".");
                writer.Write("TurnOn");
                writer.Write("()");
                writer.Write(";");
                writer.NewLine();
            }
            else if (input == Unit.turnOff)
            {
                writer.WriteIndented(variableName.VariableHighlight());
                writer.Write(".");
                writer.Write("TurnOff");
                writer.Write("()");
                writer.Write(";");
                writer.NewLine();
            }
            else if (input == Unit.toggle)
            {
                writer.WriteIndented(variableName.VariableHighlight());
                writer.Write(".");
                writer.Write("Toggle");
                writer.Write("()");
                writer.Write(";");
                writer.NewLine();
            }

            GenerateActionMethod(Unit.turnedOn, data);
            GenerateActionMethod(Unit.turnedOff, data);

            void GenerateActionMethod(ControlOutput port, ControlGenerationData data)
            {
                if (port.hasValidConnection && !data.scopeGeneratorData.TryGetValue(port, out _))
                {
                    data.scopeGeneratorData.Add(port, true);
                    writer.WriteLine("void".ConstructHighlight() + " " + GetAction(port) + "()");
                    writer.WriteLine("{");

                    using (writer.IndentedScope(data))
                    {
                        GenerateChildControl(port, data, writer);
                    }

                    writer.WriteLine("}");
                }
            }
        }
    }
}