using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Utility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(TriggerReturnEvent))]
    public class TriggerReturnEventGenerator : MethodNodeGenerator, IRequireVariables
    {
        private TriggerReturnEvent Unit => unit as TriggerReturnEvent;

        public override ControlOutput OutputPort => Unit.trigger;

        public override List<ValueOutput> OutputValues => Unit.value.Yield().ToList();

        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override MethodModifier MethodModifier => MethodModifier.None;

        public override string Name => "TriggerReturnEventRunner" + count;

        public override Type ReturnType => Unit.coroutine ? typeof(IEnumerator) : typeof(void);

        public override List<TypeParam> Parameters => new TypeParam(typeof(object), "value").Yield().ToList();

        private string name = "triggerReturnResult";

        public TriggerReturnEventGenerator(Unit unit) : base(unit) { }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.GetVariable(name);
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            writer.WriteIndented(typeof(ReturnEvent).As().CSharpName(false, true));
            writer.Write(".");
            writer.Write("Trigger");
            writer.Write("(");

            if (Unit.global)
            {
                writer.Write("null");
            }
            else
            {
                GenerateValue(Unit.target, data, writer);
            }
            writer.Write(", ");

            GenerateValue(Unit.name, data, writer);
            writer.Write(", ");

            writer.Write("(");
            writer.Write("value".VariableHighlight());
            writer.Write(") => ");
            writer.NewLine();
            writer.WriteLine("{");

            using (writer.IndentedScope(data))
            {
                writer.WriteIndented(name.VariableHighlight());
                writer.Write(" = ");
                writer.Write("value".VariableHighlight());
                writer.Write(";");
                writer.NewLine();

                if (Unit.coroutine)
                {
                    writer.WriteIndented("StartCoroutine");
                    writer.Write("(");
                    writer.Write(Name);
                    writer.Write("(");
                    writer.Write("value".VariableHighlight());
                    writer.Write(")");
                    writer.Write(")");
                    writer.Write(";");
                    writer.NewLine();
                }
                else
                {
                    writer.WriteIndented(Name);
                    writer.Write("(");
                    writer.Write("value".VariableHighlight());
                    writer.Write(")");
                    writer.Write(";");
                    writer.NewLine();
                }
            }

            writer.WriteIndented("}");
            writer.Write(", ");

            writer.Write(writer.BoolString(Unit.global));
            writer.Write(", ");

            writer.Write("new ".ConstructHighlight());
            writer.Write(typeof(object[]).As().CSharpName(false, true));
            writer.Write(" ");
            if (Unit.count > 0)
            {
                writer.NewLine();
                writer.WriteLine("{");
            }
            else
            {
                writer.Write("{");
            }

            if (Unit.count > 0)
            {
                using (writer.IndentedScope(data))
                {
                    var args = Unit.arguments;
                    for (int i = 0; i < args.Count; i++)
                    {
                        writer.WriteIndented();
                        GenerateValue(args[i], data, writer);
                        if (i < args.Count - 1)
                        {
                            writer.Write(",");
                        }
                        writer.NewLine();
                    }
                }
                writer.WriteIndented("}");
            }
            else
            {
                writer.Write("}");
            }

            writer.Write(")");
            writer.Write(";");
            writer.NewLine();

            GenerateExitControl(Unit.exit, data, writer);
        }

        public IEnumerable<FieldGenerator> GetRequiredVariables(ControlGenerationData data)
        {
            name = data.AddLocalNameInScope(name, typeof(object));
            var field = FieldGenerator.Field(AccessModifier.Private, FieldModifier.None, typeof(object), name);
            yield return field;
        }
    }
}