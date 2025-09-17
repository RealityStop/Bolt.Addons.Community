using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Utility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnDropdownValueChanged))]
    public class OnDropdownValueChangedGenerator : EventListenerMethodGenerator<OnDropdownValueChanged>
    {
        public OnDropdownValueChangedGenerator(Unit unit) : base(unit) { NameSpaces = "UnityEngine.UI"; }

        public override ControlOutput OutputPort => Unit.trigger;
        public override List<ValueOutput> OutputValues => new() { Unit.index, Unit.text };
        public override List<TypeParam> Parameters => new() { new(typeof(int), "value") };

        protected override string GetListenerSetupCode()
        {
            return $".GetComponent<{"Dropdown".TypeHighlight()}>()?.{"onValueChanged".VariableHighlight()}?.AddListener({(!Unit.coroutine ? Name : $"({"value".VariableHighlight()}) => StartCoroutine({Name}({"value".VariableHighlight()}))")});";
        }

        protected override ValueInput GetTargetValueInput()
        {
            return Unit.target;
        }

        protected override bool IsCoroutine()
        {
            return Unit.coroutine;
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (output == Unit.index)
                return MakeClickableForThisUnit("value".VariableHighlight());
            else
                return GenerateValue(Unit.target, data) + MakeClickableForThisUnit($".GetComponent<{"Dropdown".TypeHighlight()}>()?.{"text".VariableHighlight()}");
        }
    }
}