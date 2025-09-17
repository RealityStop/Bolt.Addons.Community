using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnInputFieldEndEdit))]
    public class OnInputFieldEndEditGeneratorGenerator : EventListenerMethodGenerator<OnInputFieldEndEdit>
    {
        public OnInputFieldEndEditGeneratorGenerator(Unit unit) : base(unit)
        {
        }
        public override List<ValueOutput> OutputValues => new() { Unit.value };

        public override List<TypeParam> Parameters => new() { new TypeParam(typeof(string), "value") };

        public override ControlOutput OutputPort => Unit.trigger;

        protected override bool IsCoroutine()
        {
            return Unit.coroutine;
        }

        protected override string GetListenerSetupCode()
        {
            return $".GetComponent<{"InputField".TypeHighlight()}>()?.{"onEndEdit".VariableHighlight()}?.AddListener({(!Unit.coroutine ? Name : $"({"value".VariableHighlight()}) => StartCoroutine({Name}({"value".VariableHighlight()}))")});";
        }

        protected override ValueInput GetTargetValueInput()
        {
            return Unit.target;
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return MakeClickableForThisUnit("value".VariableHighlight());
        }
    }
}
