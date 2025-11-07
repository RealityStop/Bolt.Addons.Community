using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Utility;
using System.Collections;
using System.Linq;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnAnimatorIK))]
    public class OnAnimatorIKGenerator : MethodNodeGenerator
    {
        private OnAnimatorIK Unit => unit as OnAnimatorIK;
        public override ControlOutput OutputPort => Unit.trigger;

        public override List<ValueOutput> OutputValues => new List<ValueOutput>();

        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override MethodModifier MethodModifier => MethodModifier.None;

        public override string Name => "OnAnimatorIK";

        public override Type ReturnType => Unit.coroutine ? typeof(IEnumerator) : typeof(void);

        public override List<TypeParam> Parameters => new TypeParam(typeof(int), "layerIndex").Yield().ToList();

        public OnAnimatorIKGenerator(Unit unit) : base(unit) { }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return MakeClickableForThisUnit("layerIndex".VariableHighlight());
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            data.AddLocalNameInScope("layerIndex", typeof(int));
            return GetNextUnit(Unit.trigger, data, indent);
        }
    }
}