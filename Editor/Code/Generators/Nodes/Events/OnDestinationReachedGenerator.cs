#if MODULE_AI_EXISTS
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Utility;
using System.Collections;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnDestinationReached))]
    public class OnDestinationReachedGenerator : UpdateMethodNodeGenerator
    {
        private OnDestinationReached Unit => unit as OnDestinationReached;
        public override ControlOutput OutputPort => Unit.trigger;

        public override List<ValueOutput> OutputValues => new List<ValueOutput>();

        public override Type ReturnType => Unit.coroutine ? typeof(IEnumerator) : typeof(void);

        public override List<TypeParam> Parameters => new List<TypeParam>();

        public OnDestinationReachedGenerator(Unit unit) : base(unit) { }

        public override string GenerateUpdateCode(ControlGenerationData data, int indent)
        {
            var builder = Unit.CreateClickableString();
            builder.Indent(indent);
            builder.Body(before =>
                before.Clickable("if ".ControlHighlight()).Parentheses(inner =>
                    inner.CallCSharpUtilityMethod(MakeClickableForThisUnit("DestinationReached"),
                        p1 => p1.Clickable("gameObject".VariableHighlight()),
                        p2 => p2.Ignore(GenerateValue(Unit.threshold, data)),
                        p3 => p3.Ignore(GenerateValue(Unit.requireSuccess, data))
                    )
                ),
            (body, _indent) => body.Indent(_indent).Clickable(Unit.coroutine ? $"StartCoroutine({Name + "()"})" : Name + "()"), true, indent);
            return builder;
        }

        protected override string GenerateCode(ControlInput input, ControlGenerationData data, int indent)
        {
            return GetNextUnit(Unit.trigger, data, indent);
        }
    }
}
#endif