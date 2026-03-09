using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Utility;
using System.Collections;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(ReturnEvent))]
    public class ReturnEventGenerator : AwakeMethodNodeGenerator, IRequireMethods
    {
        private ReturnEvent Unit => unit as ReturnEvent;
        public override ControlOutput OutputPort => Unit.trigger;

        public override List<ValueOutput> OutputValues => Unit.arguments;

        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override MethodModifier MethodModifier => MethodModifier.None;

        public override string Name => "ReturnEvent" + count;

        public override Type ReturnType => Unit.coroutine ? typeof(IEnumerator) : typeof(void);

        public override List<TypeParam> Parameters => new TypeParam(typeof(ReturnEventArg), "args").Yield().ToList();

        private string name = "ReturnEventRunner";
        public ReturnEventGenerator(Unit unit) : base(unit) { }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (output == Unit.eventData)
            {
                if (data.GetExpectedType() == typeof(ReturnEventArg))
                {
                    data.MarkExpectedTypeMet(typeof(ReturnEventArg));
                }
                writer.GetVariable("args");
                return;
            }

            using (writer.Cast(data.GetExpectedType(), () => data.GetExpectedType() != null && !data.IsCurrentExpectedTypeMet() && data.GetExpectedType() != typeof(object), true))
            {
                writer.InvokeMember("args".VariableHighlight(), nameof(CSharpUtility.GetArgument), writer.IntString(Unit.arguments.IndexOf(output)),
                "typeof".ConstructHighlight() + "(" + (data.GetExpectedType() ?? typeof(object)).As().CSharpName(false, true) + ")");
            }
        }

        public override void GenerateAwakeCode(ControlGenerationData data, CodeWriter writer)
        {
            writer.WriteIndented();
            writer.CallCSharpUtilityMethod("RegisterReturnEvent",
                writer.Action(() =>
                {
                    if (!Unit.global)
                        GenerateValue(Unit.target, data, writer);
                    else
                        writer.Write("null".ConstructHighlight());
                }),
                writer.Action(() => writer.Write(name.VariableHighlight())),
                writer.Action(() => GenerateValue(Unit.name, data, writer)),
                writer.Action(() => writer.Object(Unit.count)),
                writer.Action(() => writer.Object(Unit.ToString().Replace(".", "")))
            );
            writer.Write(";");
            writer.NewLine();
        }

        protected override void GenerateCode(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            GenerateChildControl(Unit.trigger, data, writer);
        }

        public IEnumerable<MethodGenerator> GetRequiredMethods(ControlGenerationData data)
        {
            name = data.AddMethodName(name);
            var method = MethodGenerator.Method(AccessModifier.Private, MethodModifier.None, typeof(void), name);
            method.AddParameter(ParameterGenerator.Parameter("args", typeof(ReturnEventArg), ParameterModifier.None));
            data.AddLocalNameInScope("args");
            method.Body(writer =>
            {
                writer.WriteIndented(Unit.coroutine ? $"StartCoroutine({name}({"args".VariableHighlight()}))" : Name + $"({"args".VariableHighlight()})");
                writer.Write(";");
                writer.NewLine();
            });
            yield return method;
        }
    }
}