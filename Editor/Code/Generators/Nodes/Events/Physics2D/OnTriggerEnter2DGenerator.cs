using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Utility;
using UnityEngine;

#if MODULE_PHYSICS_2D_EXISTS
namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnTriggerEnter2D))]
    public class OnTriggerEnter2DGenerator : UnityMethodGenerator<OnTriggerEnter2D, Collider2D>
    {
        public OnTriggerEnter2DGenerator(Unit unit) : base(unit)
        {
        }

        public override List<ValueOutput> OutputValues => new List<ValueOutput>() { Unit.collider };

        public override List<TypeParam> Parameters => new List<TypeParam>() { new TypeParam(typeof(Collider2D), "other") };


        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.GetVariable("other");
        }
    }
}
#endif