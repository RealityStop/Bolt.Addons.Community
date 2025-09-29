using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Utility;
using UnityEngine;

#if MODULE_PHYSICS_EXISTS
namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnTriggerEnter))]
    public class OnTriggerEnterGenerator : UnityMethodGenerator<OnTriggerEnter, Collider>
    {
        public OnTriggerEnterGenerator(Unit unit) : base(unit)
        {
        }

        public override List<ValueOutput> OutputValues => new List<ValueOutput>() { Unit.collider };

        public override List<TypeParam> Parameters => new List<TypeParam>() { new TypeParam(typeof(Collision), "other") };

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return MakeClickableForThisUnit("other".VariableHighlight());
        }
    }
}
#endif