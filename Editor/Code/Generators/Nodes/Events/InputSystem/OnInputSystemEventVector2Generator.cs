#if PACKAGE_INPUT_SYSTEM_EXISTS
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Utility;
using UnityEngine;
using System.Text;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine.InputSystem;
using Unity.VisualScripting.InputSystem;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnInputSystemEventVector2))]
    public sealed class OnInputSystemEventVector2Generator : MethodNodeGenerator
    {
        public OnInputSystemEventVector2Generator(Unit unit) : base(unit)
        {
        }
        private OnInputSystemEventVector2 Unit => unit as OnInputSystemEventVector2;
        public override ControlOutput OutputPort => Unit.trigger;

        public override List<ValueOutput> OutputValues => new List<ValueOutput>();

        public override AccessModifier AccessModifier => AccessModifier.None;

        public override MethodModifier MethodModifier => MethodModifier.None;

        public override string Name => "OnInputSystemEventVector2" + count;

        public override Type ReturnType => typeof(void);

        public override List<TypeParam> Parameters => new List<TypeParam>();

        protected override void GenerateValueInternal(ValueInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input == Unit.Target)
            {
                if (Unit.Target.hasValidConnection)
                {
                    base.GenerateValueInternal(input, data, writer);
                    return;
                }
                else
                {
                    var value = input.unit.defaultValues[input.key];
                    if (value == null)
                    {
                        writer.GetVariable("gameObject").GetComponent(typeof(PlayerInput));
                        return;
                    }
                }
            }
            else if (input == Unit.InputAction)
            {
                if (Unit.InputAction.hasValidConnection)
                {
                    base.GenerateValueInternal(input, data, writer);
                    return;
                }
                else
                {
                    if (!(input.unit.defaultValues[input.key] is InputAction value))
                    {
                        writer.WriteErrorDiagnostic("The problem could be that the player input component could not be found.", "Could not generate Input Action");
                        return;
                    }
                    else
                    {
                        writer.Write(value.name.As().Code(false));
                        return;
                    }
                }
            }
            base.GenerateValueInternal(input, data, writer);
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.InvokeMember("action".VariableHighlight(), "ReadValue", new CodeWriter.TypeParameter[] { typeof(Vector2) });
        }
        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (!typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType))
            {
                writer.WriteErrorDiagnostic("OnInputSystemEvents only work with ScriptGraphAssets, ScriptMachines or a ClassAsset that inherits MonoBehaviour", "Could not generate OnInputSystemEvent", WriteOptions.IndentedNewLineAfter);
                return;
            }

            var inputVariable = data.AddLocalNameInScope("playerInput", typeof(PlayerInput));
            var actionVariable = data.AddLocalNameInScope("action", typeof(InputAction));

            writer.WriteIndented("var".ConstructHighlight());
            writer.Write(" ");
            writer.Write(inputVariable.VariableHighlight());
            writer.Write(" = ");
            GenerateValue(Unit.Target, data, writer);
            writer.Write(";");
            writer.NewLine();

            writer.WriteIndented("var".ConstructHighlight());
            writer.Write(" ");
            writer.Write(actionVariable.VariableHighlight());
            writer.Write(" = ");
            writer.Write(inputVariable.VariableHighlight());
            writer.Write(".");
            writer.Write("actions".VariableHighlight());
            writer.Write(".");
            writer.Write("FindAction");
            writer.Write("(");
            GenerateValue(Unit.InputAction, data, writer);
            writer.Write(")");
            writer.Write(";");
            writer.NewLine();

            writer.WriteIndented("if".ControlHighlight());
            writer.Write(" (");
            writer.Write(GetStateCodeWriter(actionVariable.VariableHighlight()));
            writer.Write(")");
            writer.NewLine();
            writer.WriteLine("{");

            using (writer.IndentedScope(data))
            {
                GenerateChildControl(Unit.trigger, data, writer);
            }

            writer.WriteLine("}");

#if !PACKAGE_INPUT_SYSTEM_1_2_0_OR_NEWER_EXISTS
            writer.WriteLine($"{$"vector2{count}_wasRunning".VariableHighlight()} = {actionVariable.VariableHighlight()}.{"phase".VariableHighlight()} == {InputActionPhase.Started.As().Code(false)};");
#endif
        }

        private string GetStateCodeWriter(string actionVariable)
        {
#if PACKAGE_INPUT_SYSTEM_1_2_0_OR_NEWER_EXISTS
            switch (Unit.InputActionChangeType)
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
            switch (Unit.InputActionChangeType)
            {
                case InputActionChangeOption.OnPressed:
                    return actionVariable + "." + "triggered".VariableHighlight();
                case InputActionChangeOption.OnHold:
                    return actionVariable + "." + "phase".VariableHighlight() + " == " + InputActionPhase.Started.As().Code(false);
                case InputActionChangeOption.OnReleased:
                    return $"{$"vector2{count}_wasRunning".VariableHighlight()}" + " && " + actionVariable + "." + "phase".VariableHighlight() + " != " + InputActionPhase.Started.As().Code(false);
                default:
                    throw new ArgumentOutOfRangeException();
            }
#endif
        }
    }
}
#endif