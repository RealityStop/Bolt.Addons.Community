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
    [RequiresMethods]
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
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (output == Unit.eventData)
            {
                if (data.GetExpectedType() == typeof(ReturnEventArg))
                {
                    data.SetCurrentExpectedTypeMet(true, typeof(ReturnEventArg));
                }
                return MakeClickableForThisUnit("args".VariableHighlight());
            }

            var callCode = CodeBuilder.CallCSharpUtilityExtensitionMethod(Unit, MakeClickableForThisUnit("args".VariableHighlight()), MakeClickableForThisUnit(nameof(CSharpUtility.GetArgument)), Unit.arguments.IndexOf(output).As().Code(false, Unit), MakeClickableForThisUnit("typeof".ConstructHighlight() + "(" + (data.GetExpectedType() ?? typeof(object)).As().CSharpName(false, true) + ")"));
            var code = Unit.CreateClickableString().Ignore(callCode).Cast(data.GetExpectedType(), data.GetExpectedType() != null && !data.IsCurrentExpectedTypeMet() && data.GetExpectedType() != typeof(object));
            return code;
        }

        public override string GenerateAwakeCode(ControlGenerationData data, int indent)
        {
            var builder = Unit.CreateClickableString();
            builder.Indent(indent);
            if (!Unit.global)
                builder.CallCSharpUtilityMethod(MakeClickableForThisUnit("RegisterReturnEvent"),
                p => p.Ignore(GenerateValue(Unit.target, data)),
                p => p.Clickable(name),
                p => p.Ignore(GenerateValue(Unit.name, data)),
                p => p.Clickable(Unit.count.As().Code(false)),
                p => p.Clickable(Unit.ToString().Replace(".", "").As().Code(false))).EndLine();
            else
                builder.CallCSharpUtilityMethod(MakeClickableForThisUnit("RegisterReturnEvent"),
                p => p.Null(),
                p => p.Clickable(name),
                p => p.Ignore(GenerateValue(Unit.name, data)),
                p => p.Clickable(Unit.count.As().Code(false)),
                p => p.Clickable(Unit.ToString().Replace(".", "").As().Code(false))).EndLine();
            return builder;
        }

        protected override string GenerateCode(ControlInput input, ControlGenerationData data, int indent)
        {
            return GetNextUnit(Unit.trigger, data, indent);
        }

        public IEnumerable<MethodGenerator> GetRequiredMethods(ControlGenerationData data)
        {
            name = data.AddMethodName(name);
            var method = MethodGenerator.Method(AccessModifier.Private, MethodModifier.None, typeof(void), name);
            method.AddParameter(ParameterGenerator.Parameter("args", typeof(ReturnEventArg), ParameterModifier.None));
            data.AddLocalNameInScope("args");
            var builder = Unit.CreateClickableString();
            builder.Clickable(Unit.coroutine ? $"StartCoroutine({name}({"args".VariableHighlight()}))" : Name + $"({"args".VariableHighlight()})").EndLine();
            method.Body(builder);
            yield return method;
        }
    }
}