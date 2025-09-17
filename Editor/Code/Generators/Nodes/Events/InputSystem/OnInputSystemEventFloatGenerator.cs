#if PACKAGE_INPUT_SYSTEM_EXISTS
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Utility;
using UnityEngine;
using System.Text;
using UnityEngine.InputSystem;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEditor;

using Unity.VisualScripting.InputSystem;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnInputSystemEventFloat))]
    public sealed class OnInputSystemEventFloatGenerator : MethodNodeGenerator
    {
        public OnInputSystemEventFloatGenerator(Unit unit) : base(unit)
        {
        }
        private OnInputSystemEventFloat Unit => unit as OnInputSystemEventFloat;
        public override ControlOutput OutputPort => Unit.trigger;

        public override List<ValueOutput> OutputValues => new List<ValueOutput>();

        public override AccessModifier AccessModifier => AccessModifier.None;

        public override MethodModifier MethodModifier => MethodModifier.None;

        public override string Name => "OnInputSystemEventFloat" + count;

        public override Type ReturnType => typeof(void);

        public override List<TypeParam> Parameters => new List<TypeParam>();

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            if (!typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType))
            {
                return CodeBuilder.Indent(indent + 1) + MakeClickableForThisUnit(CodeUtility.ToolTip("OnInputSystemEvents only work with ScriptGraphAssets, ScriptMachines or a ClassAsset that inherits MonoBehaviour", "Could not generate OnInputSystemEvent", ""));
            }
            var output = new StringBuilder();
            var inputVariable = data.AddLocalNameInScope("playerInput", typeof(PlayerInput));
            var actionVariable = data.AddLocalNameInScope("action", typeof(InputAction));
            output.Append(CodeBuilder.Indent(indent) + MakeClickableForThisUnit("var ".ConstructHighlight() + inputVariable.VariableHighlight() + " = ") + GenerateValue(Unit.Target, data) + MakeClickableForThisUnit(";") + "\n");
            output.Append(CodeBuilder.Indent(indent) + MakeClickableForThisUnit("var ".ConstructHighlight() + actionVariable.VariableHighlight() + " = " + inputVariable.VariableHighlight() + "." + "actions".VariableHighlight() + $".FindAction(") + GenerateValue(Unit.InputAction, data) + MakeClickableForThisUnit(");") + "\n");
            output.Append(CodeBuilder.Indent(indent) + MakeClickableForThisUnit("if".ControlHighlight() + " (" + GetState(actionVariable.VariableHighlight()) + ")"));
            output.AppendLine();
            output.AppendLine(CodeBuilder.Indent(indent) + MakeClickableForThisUnit("{"));
            output.Append(GetNextUnit(Unit.trigger, data, indent + 1));
            output.AppendLine(CodeBuilder.Indent(indent) + MakeClickableForThisUnit("}"));
#if !PACKAGE_INPUT_SYSTEM_1_2_0_OR_NEWER_EXISTS
            output.AppendLine(CodeBuilder.Indent(indent) + MakeSelectableForThisUnit($"{$"float{count}_wasRunning".VariableHighlight()} = {actionVariable.VariableHighlight()}.{"phase".VariableHighlight()} == {InputActionPhase.Started.As().Code(false)};"));
#endif
            return output.ToString();
        }

        private string GetState(string actionVariable)
        {
#if PACKAGE_INPUT_SYSTEM_1_2_0_OR_NEWER_EXISTS
            switch (Unit.InputActionChangeType)
            {
                case InputActionChangeOption.OnPressed:
                    return actionVariable + ".WasPressedThisFrame()";
                case InputActionChangeOption.OnHold:
                    return actionVariable + ".IsPressed()";
                case InputActionChangeOption.OnReleased:
                    return actionVariable + ".WasReleasedThisFrame()";
                default:
                    throw new ArgumentOutOfRangeException();
            }
#else
            switch (Unit.InputActionChangeType)
            {
                case InputActionChangeOption.OnPressed:
                    return actionVariable + $".{triggered.VariableHighlight()}";
                case InputActionChangeOption.OnHold:
                    return actionVariable + $".{phase.VariableHighlight()} == {InputActionPhase.Started.As().Code(false)}";
                case InputActionChangeOption.OnReleased:
                    return $"{$"float{count}_wasRunning".VariableHighlight()}" + " && " + actionVariable + $".{phase.VariableHighlight()} != {InputActionPhase.Started.As().Code(false)}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
#endif
        }

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input == Unit.Target)
            {
                if (Unit.Target.hasValidConnection)
                {
                    data.SetExpectedType(input.type);
                    var code = GetNextValueUnit(input, data);
                    data.RemoveExpectedType();
                    return code;
                }
                else
                {
                    var value = input.unit.defaultValues[input.key];
                    if (value == null)
                    {
                        return MakeClickableForThisUnit("gameObject".VariableHighlight() + "." + $"GetComponent<{typeof(PlayerInput).As().CSharpName(false, true, true)}>()");
                    }
                    else
                    {
                        return base.GenerateValue(input, data);
                    }
                }
            }
            else if (input == Unit.InputAction)
            {
                if (Unit.InputAction.hasValidConnection)
                {
                    data.SetExpectedType(input.type);
                    var code = GetNextValueUnit(input, data);
                    data.RemoveExpectedType();
                    return code;
                }
                else
                {
                    if (input.unit.defaultValues[input.key] is not InputAction value)
                    {
                        return MakeClickableForThisUnit(CodeUtility.ToolTip("The problem could be that the player input component could not be found.", "Could not generate Input Action", "null".ConstructHighlight()));
                    }
                    else
                    {
                        return MakeClickableForThisUnit(value.name.As().Code(false));
                    }
                }
            }
            return base.GenerateValue(input, data);
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return "action".VariableHighlight() + "." + $"ReadValue<{typeof(float).As().CSharpName(false, true)}>()";
        }
    }
}
#endif