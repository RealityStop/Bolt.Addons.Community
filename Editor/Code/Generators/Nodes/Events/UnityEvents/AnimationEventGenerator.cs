using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Utility;
using System.Collections;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(BoltAnimationEvent))]
    public class BoltAnimationEventGenerator : MethodNodeGenerator
    {
        private BoltAnimationEvent Unit => unit as BoltAnimationEvent;
        public override ControlOutput OutputPort => Unit.trigger;

        public override List<ValueOutput> OutputValues => new List<ValueOutput>();

        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override MethodModifier MethodModifier => MethodModifier.None;

        public override string Name => "AnimationEvent";

        public override Type ReturnType => Unit.coroutine ? typeof(IEnumerator) : typeof(void);

        public override List<TypeParam> Parameters => new List<TypeParam>() { new TypeParam(typeof(AnimationEvent), "animationEvent") };

        public BoltAnimationEventGenerator(Unit unit) : base(unit) { }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (output == Unit.floatParameter)
            {
                writer.GetVariable("animationEvent").GetMember("floatParameter");
            }
            else if (output == Unit.intParameter)
            {
                writer.GetVariable("animationEvent").GetMember("intParameter");
            }
            else if (output == Unit.objectReferenceParameter)
            {
                writer.GetVariable("animationEvent").GetMember("objectReferenceParameter");
            }
            else if (output == Unit.stringParameter)
            {
                writer.GetVariable("animationEvent").GetMember("stringParameter");
            }
            else
            {
                base.GenerateValueInternal(output, data, writer);
            }
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            GenerateChildControl(Unit.trigger, data, writer);
        }
    }
}