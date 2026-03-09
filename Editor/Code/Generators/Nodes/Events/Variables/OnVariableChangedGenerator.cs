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
    public class OnVariableChangedGenerator : UpdateMethodNodeGenerator, IRequireVariables
    {
        private OnVariableChanged Unit => (OnVariableChanged)unit;
        public override ControlOutput OutputPort => Unit.trigger;

        public override List<ValueOutput> OutputValues => new List<ValueOutput>() { Unit.value };

        public override Type ReturnType => Unit.coroutine ? typeof(System.Collections.IEnumerator) : typeof(void);

        public override List<TypeParam> Parameters => new List<TypeParam>() { new TypeParam(typeof(object), "value") };

        private FieldGenerator field;

        public OnVariableChangedGenerator(Unit unit) : base(unit) { }


        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.GetVariable("value");
        }

        public override void GenerateUpdateCode(ControlGenerationData data, CodeWriter writer)
        {
            if (Unit.name.hasValidConnection && !SupportsNameConnection())
            {
                writer.WriteErrorDiagnostic($"{Unit.GetType().DisplayName()} does not support a name connection when Variable Kind is set to {Unit.kind}", $"Could not generate {Unit.GetType().DisplayName()}");
                return;
            }

            writer.WriteIndented("if".ControlHighlight());
            writer.Write(" (");
            if (Unit.provideInitial)
                writer.InvokeMember(field.modifiedName.VariableHighlight(), "HasValueChanged",
                    writer.Action(() => GenerateValue(Unit.name, data, writer)),
                    writer.Action(() =>
                    {
                        writer.Write("out var ".ConstructHighlight());
                        writer.Write(data.AddLocalNameInScope("result").VariableHighlight());
                    }),
                    writer.Action(() =>
                    {
                        GenerateValue(Unit.Initial, data, writer);
                    })
                );
            else
                writer.InvokeMember(field.modifiedName.VariableHighlight(), "HasValueChanged",
                    writer.Action(() => GenerateValue(Unit.name, data, writer)),
                    writer.Action(() =>
                    {
                        writer.Write("out var ".ConstructHighlight());
                        writer.Write(data.AddLocalNameInScope("result").VariableHighlight());
                    })
                );
            writer.Write(")");
            writer.NewLine();
            writer.WriteLine("{");
            using (writer.IndentedScope(data))
            {
                writer.WriteIndented(Unit.coroutine ? $"StartCoroutine({Name}({data.GetVariableName("result").VariableHighlight()}))" : Name + $"({data.GetVariableName("result").VariableHighlight()})");
                writer.Write(";");
                writer.NewLine();
            }
            writer.WriteLine("}");
        }

        private bool SupportsNameConnection()
        {
            return Unit.kind == VariableKind.Object || Unit.kind == VariableKind.Scene || Unit.kind == VariableKind.Application || Unit.kind == VariableKind.Saved;
        }

        protected override void GenerateCode(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            GenerateChildControl(Unit.trigger, data, writer);
        }

        protected override void GenerateValueInternal(ValueInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input == Unit.name)
            {
                if (!SupportsNameConnection())
                {
                    var name = (Unit.defaultValues[Unit.name.key] as string).LegalMemberName();
                    if (!data.ContainsNameInAncestorScope(name))
                    {
                        writer.Error($"{name} variable does not exist");
                        return;
                    }
                    writer.GetVariable(name);
                    return;
                }
                else if (Unit.kind == VariableKind.Object)
                {
                    writer.InvokeMember(typeof(VisualScripting.Variables), "Object", writer.Action(() => base.GenerateValueInternal(Unit.@object, data, writer)))
                    .InvokeMember(null, "Get", writer.Action(() => base.GenerateValueInternal(Unit.@name, data, writer)));
                    return;
                }
                else if (Unit.kind == VariableKind.Scene)
                {
                    if (typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType))
                    {
                        writer.InvokeMember(typeof(VisualScripting.Variables), "Scene", writer.Action(() => writer.GetMember("gameObject".VariableHighlight(), "scene")))
                        .InvokeMember(null, "Get", writer.Action(() => base.GenerateValueInternal(Unit.@name, data, writer)));
                    }
                    else
                    {
                        writer.GetMember(typeof(VisualScripting.Variables), "ActiveScene")
                        .InvokeMember(null, "Get", writer.Action(() => base.GenerateValueInternal(Unit.@name, data, writer)));
                    }
                    return;
                }
                else if (Unit.kind == VariableKind.Application)
                {
                    writer.GetMember(typeof(VisualScripting.Variables), "Application")
                    .InvokeMember(null, "Get", writer.Action(() => base.GenerateValueInternal(Unit.@name, data, writer)));
                    return;
                }
                else if (Unit.kind == VariableKind.Saved)
                {
                    writer.GetMember(typeof(VisualScripting.Variables), "Saved")
                    .InvokeMember(null, "Get", writer.Action(() => base.GenerateValueInternal(Unit.@name, data, writer)));
                    return;
                }
            }

            if (input == Unit.@object && !Unit.@object.hasValidConnection && Unit.defaultValues[Unit.@object.key] == null)
            {
                writer.GetVariable("gameObject");
                return;
            }

            base.GenerateValueInternal(input, data, writer);
        }

        public IEnumerable<FieldGenerator> GetRequiredVariables(ControlGenerationData data)
        {
            field = FieldGenerator.Field(AccessModifier.Private, FieldModifier.None, typeof(OnValueChangedLogic), "onValueChanged", new OnValueChangedLogic(Unit.provideInitial));
            field.SetNewlineLiteral(false);
            yield return field;
        }
    }
}