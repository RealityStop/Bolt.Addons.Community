using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Timer))]
    public class TimerGenerator : AwakeVariableNodeGenerator, IRequireMethods, IRequireUpdateCode
    {
        public TimerGenerator(Timer unit) : base(unit)
        {
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
            output += Name.Capitalize().First().Letter() + "_" + controlOutput.key.Capitalize().First().Letter();
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

        string tickMethodName;
        string completedMethodName;

        public IEnumerable<MethodGenerator> GetRequiredMethods(ControlGenerationData data)
        {
            if (Unit.tick.hasValidConnection)
            {
                tickMethodName = data.AddMethodName(GetAction(Unit.tick));
                var method = MethodGenerator.Method(AccessModifier.Private, MethodModifier.None, typeof(void), tickMethodName);
                method.Body(w => GenerateChildControl(Unit.tick, data, w));
                yield return method;
            }

            if (Unit.completed.hasValidConnection)
            {
                completedMethodName = data.AddMethodName(GetAction(Unit.completed));
                var method = MethodGenerator.Method(AccessModifier.Private, MethodModifier.None, typeof(void), completedMethodName);
                method.Body(w => GenerateChildControl(Unit.completed, data, w));
                yield return method;
            }
        }

        public override void GenerateAwakeCode(ControlGenerationData data, CodeWriter writer)
        {
            if (!Unit.completed.hasValidConnection && !Unit.tick.hasValidConnection) return;

            string turnedOnCallback = Unit.completed.hasValidConnection ? completedMethodName : null;
            string turnedOffCallback = Unit.tick.hasValidConnection ? tickMethodName : null;

            writer.WriteIndented(Name.VariableHighlight());
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

            writer.WriteEnd(EndWriteOptions.All);
        }

        protected override void GenerateCode(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input == Unit.start)
            {
                writer.WriteIndented(Name.VariableHighlight());
                writer.Write(".");
                writer.Write("StartTimer");
                writer.Write("(");
                GenerateValue(Unit.duration, data, writer);
                writer.Write(", ");
                GenerateValue(Unit.unscaledTime, data, writer);
                writer.Write(")");
                writer.Write(";");
                writer.NewLine();

                GenerateExitControl(Unit.started, data, writer);
            }
            else if (input == Unit.pause)
            {
                writer.WriteIndented(Name.VariableHighlight());
                writer.Write(".");
                writer.Write("PauseTimer");
                writer.Write("()");
                writer.Write(";");
                writer.NewLine();
            }
            else if (input == Unit.resume)
            {
                writer.WriteIndented(Name.VariableHighlight());
                writer.Write(".");
                writer.Write("ResumeTimer");
                writer.Write("()");
                writer.Write(";");
                writer.NewLine();
            }
            else if (input == Unit.toggle)
            {
                writer.WriteIndented(Name.VariableHighlight());
                writer.Write(".");
                writer.Write("ToggleTimer");
                writer.Write("()");
                writer.Write(";");
                writer.NewLine();
            }
        }

        public void GenerateUpdateCode(ControlGenerationData data, CodeWriter writer)
        {
            writer.WriteIndented().GetVariable(Name).InvokeMember(null, "Update").WriteEnd(EndWriteOptions.LineEnd);
        }
    }
}