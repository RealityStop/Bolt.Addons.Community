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
                return MakeSelectableForThisUnit("hit".VariableHighlight() + "." + "collider".VariableHighlight());
            }
            else if (output == Unit.controller)
            {
                return MakeSelectableForThisUnit("hit".VariableHighlight() + "." + "controller".VariableHighlight());
            }
            else if (output == Unit.moveDirection)
            {
                return MakeSelectableForThisUnit("hit".VariableHighlight() + "." + "moveDirection".VariableHighlight());
            }
            else if (output == Unit.moveLength)
            {
                return MakeSelectableForThisUnit("hit".VariableHighlight() + "." + "moveLength".VariableHighlight());
            }
            else if (output == Unit.normal)
            {
                return MakeSelectableForThisUnit("hit".VariableHighlight() + "." + "normal".VariableHighlight());
            }
            else if (output == Unit.point)
            {
                return MakeSelectableForThisUnit("hit".VariableHighlight() + "." + "point".VariableHighlight());
            }
            else
            {
                return MakeSelectableForThisUnit("hit".VariableHighlight());
            }
        }
    }
}
#endif