using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Utility;
using System.Collections;
using System.Linq;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnAnimatorMove))]
    public class OnAnimatorMoveGenerator : MethodNodeGenerator
    {
        private OnAnimatorMove Unit => unit as OnAnimatorMove;
        public override ControlOutput OutputPort => Unit.trigger;

        public override List<ValueOutput> OutputValues => new List<ValueOutput>();

        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override MethodModifier MethodModifier => MethodModifier.None;

        public override string Name => "OnAnimatorMove";

        public override Type ReturnType => Unit.coroutine ? typeof(IEnumerator) : typeof(void);

        public override List<TypeParam> Parameters => new List<TypeParam>();

        public OnAnimatorMoveGenerator(Unit unit) : base(unit) { }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return GetNextUnit(Unit.trigger, data, indent);
        }
    }
}