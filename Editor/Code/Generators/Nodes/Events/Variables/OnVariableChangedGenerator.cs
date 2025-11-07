using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Utility;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnVariableChanged))]
    [RequiresVariables]
    public class OnVariableChangedGenerator : UpdateMethodNodeGenerator, IRequireVariables
    {
        private OnVariableChanged Unit => (OnVariableChanged)unit;
        public override ControlOutput OutputPort => Unit.trigger;

        public override List<ValueOutput> OutputValues => new List<ValueOutput>() { Unit.value };

        public override Type ReturnType => Unit.coroutine ? typeof(System.Collections.IEnumerator) : typeof(void);

        public override List<TypeParam> Parameters => new List<TypeParam>() { new TypeParam(typeof(object), "value") };

        private string name = "onValueChanged";

        public OnVariableChangedGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return MakeClickableForThisUnit("value".VariableHighlight());
        }

        public override string GenerateUpdateCode(ControlGenerationData data, int indent)
        {
            if (Unit.name.hasValidConnection && !SupportsNameConnection()) return CodeBuilder.Indent(indent) + MakeClickableForThisUnit(CodeUtility.ErrorTooltip($"{Unit.GetType().DisplayName()} does not support a name connection when Variable Kind is set to {Unit.kind}", $"Could not generate {Unit.GetType().DisplayName()}", ""));

            var output = Unit.CreateClickableString().Indent(indent);
            if (Unit.provideInitial)
                output.Body(
                    before => before.Clickable("if ".ControlHighlight()).Parentheses(condition => condition.InvokeMember(name.VariableHighlight(), "HasValueChanged", p1 => p1.Ignore(GenerateValue(Unit.name, data)), p2 => p2.Clickable("out var ".ConstructHighlight() + data.AddLocalNameInScope("result").VariableHighlight()), p3 => p3.Ignore(Unit.GenerateValue(Unit.Initial, data)))),
                    (inside, indent) => inside.Indent(indent).Clickable(Unit.coroutine ? $"StartCoroutine({Name}({data.GetVariableName("result").VariableHighlight()}))" : Name + $"({data.GetVariableName("result").VariableHighlight()})").EndLine(),
                    true, indent
                );
            else
                output.Body(
                    before => before.Clickable("if ".ControlHighlight()).Parentheses(condition => condition.InvokeMember(name.VariableHighlight(), "HasValueChanged", p1 => p1.Ignore(GenerateValue(Unit.name, data)), p2 => p2.Clickable("out var ".ConstructHighlight() + data.AddLocalNameInScope("result").VariableHighlight()))),
                    (inside, indent) => inside.Indent(indent).Clickable(Unit.coroutine ? $"StartCoroutine({Name}({data.GetVariableName("result").VariableHighlight()}))" : Name + $"({data.GetVariableName("result").VariableHighlight()})").EndLine(),
                    true, indent
                );
            return output;
        }

        private bool SupportsNameConnection()
        {
            return Unit.kind == VariableKind.Object || Unit.kind == VariableKind.Scene || Unit.kind == VariableKind.Application || Unit.kind == VariableKind.Saved;
        }

        protected override string GenerateCode(ControlInput input, ControlGenerationData data, int indent)
        {
            return GetNextUnit(Unit.trigger, data, indent);
        }

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input == Unit.name)
            {
                if (!SupportsNameConnection())
                {
                    var name = (Unit.defaultValues[Unit.name.key] as string).LegalMemberName();
                    if (!data.ContainsNameInAnyScope(name))
                    {
                        return MakeClickableForThisUnit($"/* {name} variable does not exist */".WarningHighlight());
                    }
                    return MakeClickableForThisUnit(name.VariableHighlight());
                }
                else if (Unit.kind == VariableKind.Object)
                {
                    var builder = Unit.CreateClickableString();
                    builder.InvokeMember(typeof(VisualScripting.Variables), "Object", p1 => p1.Ignore(GenerateValue(Unit.@object, data)));
                    builder.Dot().MethodCall("Get", p2 => p2.Ignore(base.GenerateValue(Unit.name, data)));
                    return builder;
                }
                else if (Unit.kind == VariableKind.Scene)
                {
                    var builder = Unit.CreateClickableString();
                    builder.InvokeMember(typeof(VisualScripting.Variables), "Scene", p1 => p1.GetMember("gameObject".VariableHighlight(), "scene"));
                    builder.Dot().MethodCall("Get", p2 => p2.Ignore(base.GenerateValue(Unit.name, data)));
                    return builder;
                }
                else if (Unit.kind == VariableKind.Application)
                {
                    var builder = Unit.CreateClickableString();
                    builder.GetMember(typeof(VisualScripting.Variables), "Application");
                    builder.Dot().MethodCall("Get", p1 => p1.Ignore(base.GenerateValue(Unit.name, data)));
                    return builder;
                }
                else if (Unit.kind == VariableKind.Saved)
                {
                    var builder = Unit.CreateClickableString();
                    builder.GetMember(typeof(VisualScripting.Variables), "Saved");
                    builder.Dot().MethodCall("Get", p1 => p1.Ignore(base.GenerateValue(Unit.name, data)));
                    return builder;
                }
            }

            if (input == Unit.@object && !Unit.@object.hasValidConnection && Unit.defaultValues[Unit.@object.key] == null)
                return MakeClickableForThisUnit("gameObject".VariableHighlight());

            return base.GenerateValue(input, data);
        }

        public IEnumerable<FieldGenerator> GetRequiredVariables(ControlGenerationData data)
        {
            name = data.AddLocalNameInScope(name);
            var field = FieldGenerator.Field(AccessModifier.Private, FieldModifier.None, typeof(OnValueChangedLogic), name, new OnValueChangedLogic(Unit.provideInitial));
            field.SetNewlineLiteral(false);
            yield return field;
        }
    }
}