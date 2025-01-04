using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Utility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(FixedUpdate))]
    public class FixedUpdateGenerator : MethodNodeGenerator
    {
        public FixedUpdateGenerator(FixedUpdate unit) : base(unit) { }

        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override MethodModifier MethodModifier => MethodModifier.None;

        public override string Name => "FixedUpdate";

        public override Type ReturnType => typeof(void);
        private FixedUpdate Unit => unit as FixedUpdate;
        public override List<TypeParam> Parameters => new List<TypeParam>();

        public override ControlOutput OutputPort => Unit.trigger;

        public override List<ValueOutput> OutputValues => new List<ValueOutput>();

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return GetNextUnit(Unit.trigger, Data ?? data, indent);
        }
    }
}
