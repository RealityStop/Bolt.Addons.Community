using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Utility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnToggleValueChanged))]
    public class OnToggleValueChangedGenerator : EventListenerMethodGenerator<OnToggleValueChanged>
    {
        public OnToggleValueChangedGenerator(Unit unit) : base(unit) { }

        public override IEnumerable<string> GetNamespaces()
        {
            yield return "UnityEngine.UI";
        }

        public override List<ValueOutput> OutputValues => new List<ValueOutput>() { Unit.value };

        public override List<TypeParam> Parameters => new List<TypeParam>() { new TypeParam(typeof(bool), "value") };

        public override ControlOutput OutputPort => Unit.trigger;

        protected override bool IsCoroutine()
        {
            return Unit.coroutine;
        }

        protected override string GetListenerSetupCode()
        {
            return $".GetComponent<{"Toggle".TypeHighlight()}>()?.{"onValueChanged".VariableHighlight()}?.AddListener({(!Unit.coroutine ? Name : $"({"value".VariableHighlight()}) => StartCoroutine({Name}({"value".VariableHighlight()}))")});";
        }

        protected override ValueInput GetTargetValueInput()
        {
            return Unit.target;
        }
        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.GetVariable("value");
        }
    }
}
