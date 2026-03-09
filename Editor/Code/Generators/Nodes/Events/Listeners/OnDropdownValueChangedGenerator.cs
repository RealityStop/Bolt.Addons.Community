using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Utility;
using UnityEngine.UI;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnDropdownValueChanged))]
    public class OnDropdownValueChangedGenerator : EventListenerMethodGenerator<OnDropdownValueChanged>
    {
        public OnDropdownValueChangedGenerator(Unit unit) : base(unit) { }

        public override IEnumerable<string> GetNamespaces()
        {
            yield return "UnityEngine.UI";
        }

        public override ControlOutput OutputPort => Unit.trigger;
        public override List<ValueOutput> OutputValues => new List<ValueOutput>() { Unit.index, Unit.text };
        public override List<TypeParam> Parameters => new List<TypeParam>() { new TypeParam(typeof(int), "value") };

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

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (output == Unit.index)
            {
                writer.GetVariable("value");
            }
            else
            {
                GenerateValue(Unit.target, data, writer);
                writer.GetComponent(typeof(Dropdown)).Write("?").GetMember("text");
            }
        }
    }
}