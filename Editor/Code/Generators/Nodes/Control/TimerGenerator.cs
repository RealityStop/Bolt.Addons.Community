using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Timer))]
    public class TimerGenerator : VariableNodeGenerator
    {
        public TimerGenerator(Timer unit) : base(unit)
        {
            variableName = Name;
        }

        public override IEnumerable<string> GetNamespaces()
        {
            yield return "Unity.VisualScripting.Community";
        }

        private Timer Unit => unit as Timer;
        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override FieldModifier FieldModifier => FieldModifier.None;

        public override string Name => "timer" + count;

        public override Type Type => typeof(TimerLogic);

        public override object DefaultValue => new TimerLogic();

        public override bool HasDefaultValue => true;

        public override bool Literal => false;

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
            writer.GetVariable(Name);

            if (output == Unit.elapsedSeconds)
            {
                writer.GetMember("Elapsed");
            }
            else if (output == Unit.elapsedRatio)
            {
                writer.GetMember("ElapsedPercentage");
            }
            else if (output == Unit.remainingSeconds)
            {
                writer.GetMember("Remaining");
            }
            else if (output == Unit.remainingRatio)
            {
                writer.GetMember("RemainingPercentage");
            }
            else
            {
                base.GenerateValueInternal(output, data, writer);
            }
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            variableName = Name;
            if (!typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType))
            {
                writer.WriteErrorDiagnostic("Timer only works with ScriptGraphAssets, ScriptMachines or a ClassAsset that inherits MonoBehaviour", "Could not generate Timer", WriteOptions.IndentedNewLineAfter);
                return;
            }

            if (Unit.start.hasValidConnection && !data.scopeGeneratorData.TryGetValue(Unit.start, out _))
            {
                data.scopeGeneratorData.Add(Unit.start, true);
                string turnedOnCallback = Unit.completed.hasValidConnection ? GetAction(Unit.completed) : null;
                string turnedOffCallback = Unit.tick.hasValidConnection ? GetAction(Unit.tick) : null;

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

            if (input == Unit.start)
            {
                var action = GetAction(Unit.started);
                writer.WriteIndented(variableName.VariableHighlight());
                writer.Write(".");
                writer.Write("StartTimer");
                writer.Write("(");
                GenerateValue(Unit.duration, data, writer);
                writer.Write(", ");
                GenerateValue(Unit.unscaledTime, data, writer);
                if (!string.IsNullOrEmpty(action))
                {
                    writer.Write(", ");
                    writer.Write(action);
                }
                writer.Write(")");
                writer.Write(";");
                writer.NewLine();
            }
            else if (input == Unit.pause)
            {
                writer.WriteIndented(variableName.VariableHighlight());
                writer.Write(".");
                writer.Write("PauseTimer");
                writer.Write("()");
                writer.Write(";");
                writer.NewLine();
            }
            else if (input == Unit.resume)
            {
                writer.WriteIndented(variableName.VariableHighlight());
                writer.Write(".");
                writer.Write("ResumeTimer");
                writer.Write("()");
                writer.Write(";");
                writer.NewLine();
            }
            else if (input == Unit.toggle)
            {
                writer.WriteIndented(variableName.VariableHighlight());
                writer.Write(".");
                writer.Write("ToggleTimer");
                writer.Write("()");
                writer.Write(";");
                writer.NewLine();
            }

            GenerateActionMethod(Unit.started, data);
            GenerateActionMethod(Unit.tick, data);
            GenerateActionMethod(Unit.completed, data);

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