using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Utility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public abstract class EventListenerMethodGenerator<TUnit> : AwakeMethodNodeGenerator where TUnit : Unit
    {
        protected EventListenerMethodGenerator(Unit unit) : base(unit) { }
        protected TUnit Unit => unit as TUnit;
        public override Type ReturnType => IsCoroutine() ? typeof(IEnumerator) : typeof(void);
        public override List<ValueOutput> OutputValues => new();
        public override List<TypeParam> Parameters => new();

        public override AccessModifier AccessModifier => AccessModifier.None;

        protected abstract bool IsCoroutine();
        protected abstract string GetListenerSetupCode();

        public override string GenerateAwakeCode(ControlGenerationData data, int indent)
        {
            if (!typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType)) return MakeSelectableForThisUnit(CodeUtility.ToolTip($"{unit.GetType().DisplayName()} only works with ScriptGraphAssets, ScriptMachines or a ClassAsset that inherits MonoBehaviour", $"Could not generate {unit.GetType().DisplayName()}", ""));
            return CodeBuilder.Indent(indent) + (GetTargetValueInput() != null ? GenerateValue(GetTargetValueInput(), data) + MakeSelectableForThisUnit(GetListenerSetupCode()) : MakeSelectableForThisUnit(GetListenerSetupCode()));
        }
        
        protected abstract ValueInput GetTargetValueInput();

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input.hasValidConnection)
            {
                return GetNextValueUnit(input, data);
            }
            else if (input.hasDefaultValue)
            {
                if (input.type.Is().UnityObject() && !input.hasValidConnection && input.unit.defaultValues[input.key] == null && input.nullMeansSelf) return MakeSelectableForThisUnit("gameObject".VariableHighlight());
                return input.unit.defaultValues[input.key].As().Code(true, unit, true, true, "", true, true);
            }
            else
            {
                return MakeSelectableForThisUnit($"/* \"{input.key} Requires Input\" */".WarningHighlight());
            }
        }
    }
}