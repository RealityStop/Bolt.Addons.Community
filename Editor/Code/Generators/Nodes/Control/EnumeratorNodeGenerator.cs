
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Utility;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(EnumeratorNode))]
    public class EnumeratorNodeGenerator : MethodNodeGenerator
    {
        private EnumeratorNode Unit => (EnumeratorNode)unit;
        public override ControlOutput OutputPort => Unit.trigger;

        public override List<ValueOutput> OutputValues => new List<ValueOutput>();

        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override MethodModifier MethodModifier => MethodModifier.None;

        public override string Name => "Enumerator" + count;

        public override Type ReturnType => typeof(System.Collections.IEnumerator);

        public override List<TypeParam> Parameters => new List<TypeParam>();

        public EnumeratorNodeGenerator(Unit unit) : base(unit) { }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.Write(Name + "()");
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            GenerateChildControl(OutputPort, data, writer);
        }
    }
}
