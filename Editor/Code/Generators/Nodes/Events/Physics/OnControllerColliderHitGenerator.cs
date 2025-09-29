using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Utility;
using UnityEngine;

#if MODULE_PHYSICS_EXISTS
namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnControllerColliderHit))]
    public class OnControllerColliderHitGenerator : UnityMethodGenerator<OnControllerColliderHit, ControllerColliderHit>
    {
        public OnControllerColliderHitGenerator(Unit unit) : base(unit)
        {
        }

        public override List<ValueOutput> OutputValues => new List<ValueOutput>() { Unit.collider, Unit.controller, Unit.moveDirection, Unit.moveLength, Unit.normal, Unit.point, Unit.data };

        public override List<TypeParam> Parameters => new List<TypeParam>() { new TypeParam(typeof(ControllerColliderHit), "hit") };

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (output == Unit.collider)
            {
                return MakeClickableForThisUnit("hit".VariableHighlight() + "." + "collider".VariableHighlight());
            }
            else if (output == Unit.controller)
            {
                return MakeClickableForThisUnit("hit".VariableHighlight() + "." + "controller".VariableHighlight());
            }
            else if (output == Unit.moveDirection)
            {
                return MakeClickableForThisUnit("hit".VariableHighlight() + "." + "moveDirection".VariableHighlight());
            }
            else if (output == Unit.moveLength)
            {
                return MakeClickableForThisUnit("hit".VariableHighlight() + "." + "moveLength".VariableHighlight());
            }
            else if (output == Unit.normal)
            {
                return MakeClickableForThisUnit("hit".VariableHighlight() + "." + "normal".VariableHighlight());
            }
            else if (output == Unit.point)
            {
                return MakeClickableForThisUnit("hit".VariableHighlight() + "." + "point".VariableHighlight());
            }
            else
            {
                return MakeClickableForThisUnit("hit".VariableHighlight());
            }
        }
    }
}
#endif