using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Utility;
using UnityEngine;

#if MODULE_PHYSICS_EXISTS
namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnJointBreak))]
    public class OnJointBreakGenerator : UnityMethodGenerator<OnJointBreak, float>
    {
        public OnJointBreakGenerator(Unit unit) : base(unit)
        {
        }

        public override List<ValueOutput> OutputValues => new List<ValueOutput>() { Unit.breakForce };

        public override List<TypeParam> Parameters => new List<TypeParam>() { new TypeParam(typeof(float), "breakForce") };


        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.GetVariable("breakForce");
        }
    }
}
#endif