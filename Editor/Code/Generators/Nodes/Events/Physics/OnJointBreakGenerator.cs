using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Utility;
using UnityEngine;

#if MODULE_PHYSICS_EXISTS
namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnJointBreak))]
    public class OnJointBreakGenerator : UnityMethodGenerator<OnJointBreak, float>
    {
        public OnJointBreakGenerator(Unit unit) : base(unit)
        {
        }

        public override List<ValueOutput> OutputValues => new List<ValueOutput>() { Unit.breakForce };

        public override List<TypeParam> Parameters => new List<TypeParam>() { new TypeParam(typeof(float), "breakForce") };

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return MakeClickableForThisUnit("breakForce".VariableHighlight());
        }
    }
}
#endif