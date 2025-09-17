using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Utility;
using System.Collections;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(BoltAnimationEvent))]
    public class BoltAnimationEventGenerator : MethodNodeGenerator
    {
        private BoltAnimationEvent Unit => unit as BoltAnimationEvent;
        public override ControlOutput OutputPort => Unit.trigger;

        public override List<ValueOutput> OutputValues => new();

        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override MethodModifier MethodModifier => MethodModifier.None;

        public override string Name => "AnimationEvent";

        public override Type ReturnType => Unit.coroutine ? typeof(IEnumerator) : typeof(void);

        public override List<TypeParam> Parameters => new List<TypeParam>() { new TypeParam(typeof(AnimationEvent), "animationEvent") };

        public BoltAnimationEventGenerator(Unit unit) : base(unit) { }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (output == Unit.floatParameter)
            {
                return MakeClickableForThisUnit("animationEvent".VariableHighlight() + "." + "floatParameter".VariableHighlight());
            }
            else if (output == Unit.intParameter)
            {
                return MakeClickableForThisUnit("animationEvent".VariableHighlight() + "." + "intParameter".VariableHighlight());
            }
            else if (output == Unit.objectReferenceParameter)
            {
                return MakeClickableForThisUnit("animationEvent".VariableHighlight() + "." + "objectReferenceParameter".VariableHighlight());
            }
            else if (output == Unit.stringParameter)
            {
                return MakeClickableForThisUnit("animationEvent".VariableHighlight() + "." + "stringParameter".VariableHighlight());
            }
            return base.GenerateValue(output, data);
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return GetNextUnit(Unit.trigger, data, indent);
        }
    }
}