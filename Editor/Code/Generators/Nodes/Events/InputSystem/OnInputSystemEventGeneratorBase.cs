#if PACKAGE_INPUT_SYSTEM_EXISTS
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Utility;
using Unity.VisualScripting.InputSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.VisualScripting.Community.CSharp
{
    public abstract class OnInputSystemEventGeneratorBase<TUnit>
        : MethodNodeGenerator, IRequireAwakeCode, IRequireVariables
        where TUnit : Unit
    {
        protected const string VariableID = "InputSystem_PlayerInput_Variable";

        protected OnInputSystemEventGeneratorBase(Unit unit) : base(unit)
        {
        }

        protected TUnit TypedUnit => unit as TUnit;

        protected abstract ValueInput Target { get; }
        protected abstract ValueInput InputAction { get; }
        protected abstract ControlOutput Trigger { get; }
        protected abstract InputActionChangeOption ChangeType { get; }
        protected abstract string WasRunningVariablePrefix { get; }

        bool shouldGenerateAwake;

        public override ControlOutput OutputPort => Trigger;

        public override AccessModifier AccessModifier => AccessModifier.None;
        public override MethodModifier MethodModifier => MethodModifier.None;
        public override Type ReturnType => typeof(void);
        public override List<TypeParam> Parameters => new List<TypeParam>();

        public override IEnumerable<string> GetNamespaces()
        {
            yield return "UnityEngine.InputSystem";
        }

        protected override void GenerateValueInternal(ValueInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input == Target)
            {
                if (Target.hasValidConnection)
                {
                    base.GenerateValueInternal(input, data, writer);
                    return;
                }

                if (input.unit.defaultValues[input.key] == null)
                {
                    writer.GetVariable("gameObject").GetComponent(typeof(PlayerInput));
                    return;
                }
            }

            if (input == InputAction)
            {
                if (InputAction.hasValidConnection)
                {
                    base.GenerateValueInternal(input, data, writer);
                    return;
                }

                if (!(input.unit.defaultValues[input.key] is InputAction value))
                {
                    writer.WriteErrorDiagnostic(
                        "The problem could be that the player input component could not be found or is set to <None>.",
                        "Could not generate Input Action"
                    );
                    return;
                }

                writer.Object(value.name);
                return;
            }

            base.GenerateValueInternal(input, data, writer);
        }

        protected string ResolveVariable(ControlGenerationData data, CodeWriter writer)
        {
            if (!Target.hasValidConnection &&
                Target.unit.defaultValues[Target.key] == null &&
                data.globalGeneratorData.TryGetValue(VariableID, out var field))
            {
                return (field as FieldGenerator).modifiedName;
            }

            var local = data.AddLocalNameInScope("playerInput", typeof(PlayerInput));

            writer.WriteIndented("var".ConstructHighlight());
            writer.Write(" ");
            writer.Write(local.VariableHighlight());
            writer.Write(" = ");
            GenerateValue(Target, data, writer);
            writer.Write(";");
            writer.NewLine();

            return local;
        }

        protected string GetStateExpression(string actionVariable)
        {
#if PACKAGE_INPUT_SYSTEM_1_2_0_OR_NEWER_EXISTS
            switch (ChangeType)
            {
                case InputActionChangeOption.OnPressed:
                    return actionVariable + "." + "WasPressedThisFrame" + "()";
                case InputActionChangeOption.OnHold:
                    return actionVariable + "." + "IsPressed" + "()";
                case InputActionChangeOption.OnReleased:
                    return actionVariable + "." + "WasReleasedThisFrame" + "()";
                default:
                    throw new ArgumentOutOfRangeException();
            }
#else
            switch (ChangeType)
            {
                case InputActionChangeOption.OnPressed:
                    return actionVariable + "." + "triggered".VariableHighlight();
                case InputActionChangeOption.OnHold:
                    return actionVariable + "." + "phase".VariableHighlight() + " == " +
                           InputActionPhase.Started.As().Code(false);
                case InputActionChangeOption.OnReleased:
                    return $"{WasRunningVariablePrefix}{count}_wasRunning".VariableHighlight() +
                           " && " + actionVariable + "." + "phase".VariableHighlight() +
                           " != " + InputActionPhase.Started.As().Code(false);
                default:
                    throw new ArgumentOutOfRangeException();
            }
#endif
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (!typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType))
            {
                writer.WriteErrorDiagnostic(
                    "OnInputSystemEvents only work with ScriptGraphAssets, ScriptMachines or a ClassAsset that inherits MonoBehaviour",
                    "Could not generate OnInputSystemEvent",
                    WriteOptions.IndentedNewLineAfter
                );
                return;
            }

            var inputVariable = ResolveVariable(data, writer);
            var actionVariable = data.AddLocalNameInScope("action", typeof(InputAction));

            writer.WriteIndented("var".ConstructHighlight());
            writer.Write(" ");
            writer.Write(actionVariable.VariableHighlight());
            writer.Write(" = ");

            if (!InputAction.hasValidConnection)
            {
                writer.Write(inputVariable.VariableHighlight());
                writer.Write(".");
                writer.Write("actions".VariableHighlight());
                writer.Write(".");
                writer.Write("FindAction");
                writer.Write("(");
                GenerateValue(InputAction, data, writer);
                writer.Write(")");
            }
            else
            {
                GenerateValue(InputAction, data, writer);
            }

            writer.Write(";");
            writer.NewLine();

            writer.WriteIndented("if".ControlHighlight());
            writer.Write(" (");
            writer.Write(GetStateExpression(actionVariable.VariableHighlight()));
            writer.Write(")");
            writer.NewLine();

            writer.WriteLine("{");

            using (writer.IndentedScope(data))
            {
                GenerateChildControl(Trigger, data, writer);
            }

            writer.WriteLine("}");

#if !PACKAGE_INPUT_SYSTEM_1_2_0_OR_NEWER_EXISTS
            writer.WriteLine(
                $"{WasRunningVariablePrefix}{count}_wasRunning".VariableHighlight() +
                " = " + actionVariable.VariableHighlight() +
                "." + "phase".VariableHighlight() +
                " == " + InputActionPhase.Started.As().Code(false) + ";"
            );
#endif
        }

        public void GenerateAwakeCode(ControlGenerationData data, CodeWriter writer)
        {
            if (!shouldGenerateAwake)
                return;

            writer.SetVariable(
                "playerInput",
                writer.Action(() => writer.GetVariable("gameObject").GetComponent(typeof(PlayerInput)))
            );
        }

        public IEnumerable<FieldGenerator> GetRequiredVariables(ControlGenerationData data)
        {
            if (Target.hasValidConnection || Target.unit.defaultValues[Target.key] != null || data.globalGeneratorData.ContainsKey(VariableID))
                yield break;

            shouldGenerateAwake = true;

            var field = FieldGenerator.Field(AccessModifier.Private, FieldModifier.None, typeof(PlayerInput), "playerInput");

            data.globalGeneratorData.Add(VariableID, field);
            yield return field;
        }
    }
}
#endif