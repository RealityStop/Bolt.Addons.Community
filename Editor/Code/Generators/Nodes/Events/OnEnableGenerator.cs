using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Utility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnEnable))]
    public class OnEnableGenerator : MethodNodeGenerator<OnEnable>
    {
        public OnEnableGenerator(OnEnable unit) : base(unit) { }
        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override MethodModifier MethodModifier => MethodModifier.None;

        public override string Name => "OnEnable";

        public override Type ReturnType => typeof(void);

        public override List<TypeParam> Parameters => new List<TypeParam>();


        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return GetNextUnit(Unit.trigger, Data ?? data, indent);
        }
    }
}
