using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Utility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(TriggerReturnEvent))]
    [RequiresVariables]
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

        public override string MethodBody => GetNextUnit(Unit.trigger, Data, indent);

        private string name = "triggerReturnResult";


        public TriggerReturnEventGenerator(Unit unit) : base(unit) { }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return MakeClickableForThisUnit(name.VariableHighlight());
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var builder = Unit.CreateClickableString();

            builder.InvokeMember(
                typeof(ReturnEvent),
                "Trigger",
                // 1: target
                p => GenerateTarget(p, data),

                // 2: name
                p => p.Ignore(GenerateValue(Unit.name, data)),

                // 3: callback
                p => p.Body(
                    before => before.Clickable($"({"value".VariableHighlight()}) =>"),
                    (inner, i) =>
                    {
                        inner
                            .Indent(i)
                            .Clickable(name.VariableHighlight())
                            .Equal(true)
                            .Clickable("value".VariableHighlight())
                            .EndLine();

                        var call = Unit.coroutine
                            ? $"StartCoroutine({Name}({"value".VariableHighlight()}))"
                            : $"{Name}({"value".VariableHighlight()})";

                        inner
                            .Indent(i)
                            .Clickable(call)
                            .EndLine();
                    },
                    true,
                    indent,
                    false
                ),

                // 4: global
                p => p.Clickable(Unit.global.As().Code(false)),

                // 5: args
                p => p.Clickable(
                    "new ".ConstructHighlight() + typeof(object[]).As().CSharpName(false, true)
                ).Braces(
                    inner => inner.Ignore(string.Join(", ", Unit.arguments.Select(arg => GenerateValue(arg, data)))),
                    Unit.count > 0,
                    indent
                )
            ).EndLine();

            builder.Ignore(GetNextUnit(Unit.exit, data, indent));

            return builder;
        }

        public void GenerateTarget(ClickableStringBuilder parameter, ControlGenerationData data)
        {
            if (Unit.global)
            {
                parameter.Null();
                return;
            }
            parameter.Ignore(GenerateValue(Unit.target, data));
        }

        public IEnumerable<FieldGenerator> GetRequiredVariables(ControlGenerationData data)
        {
            name = data.AddLocalNameInScope(name, typeof(object));
            var field = FieldGenerator.Field(AccessModifier.Private, FieldModifier.None, typeof(object), name);
            yield return field;
        }
    }
}