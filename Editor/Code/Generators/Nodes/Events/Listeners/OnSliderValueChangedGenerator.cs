using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Utility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnSliderValueChanged))]
    public class OnSliderValueChangedGenerator : EventListenerMethodGenerator<OnSliderValueChanged>
    {
        public OnSliderValueChangedGenerator(Unit unit) : base(unit) { NameSpaces = "UnityEngine.UI"; }
        public override List<ValueOutput> OutputValues => new() { Unit.value };

        public override List<TypeParam> Parameters => new() { new TypeParam(typeof(float), "value") };

        public override ControlOutput OutputPort => Unit.trigger;

        protected override bool IsCoroutine()
        {
            return Unit.coroutine;
        }

        protected override string GetListenerSetupCode()
        {
            return $".GetComponent<{"Slider".TypeHighlight()}>()?.{"onValueChanged".VariableHighlight()}?.AddListener({(!Unit.coroutine ? Name : $"({"value".VariableHighlight()}) => StartCoroutine({Name}({"value".VariableHighlight()}))")});";
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
