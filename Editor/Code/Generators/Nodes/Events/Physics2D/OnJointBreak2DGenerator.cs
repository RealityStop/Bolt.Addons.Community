using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Utility;
using UnityEngine;

#if MODULE_PHYSICS_EXISTS
namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnJointBreak2D))]
    public class OnJointBreak2DGenerator : UnityMethodGenerator<OnJointBreak2D, Joint2D>
    {
        public OnJointBreak2DGenerator(Unit unit) : base(unit)
        {
        }

        public override List<ValueOutput> OutputValues => new List<ValueOutput>() { Unit.breakForce, Unit.breakTorque, Unit.connectedBody, Unit.reactionForce, Unit.reactionTorque, Unit.joint };

        public override List<TypeParam> Parameters => new List<TypeParam>() { new TypeParam(typeof(Joint2D), "brokenJoint") };

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (output == Unit.breakForce)
            {
                return MakeSelectableForThisUnit("brokenJoint".VariableHighlight() + "." + "breakForce".VariableHighlight());
            }
            else if (output == Unit.breakTorque)
            {
                return MakeSelectableForThisUnit("brokenJoint".VariableHighlight() + "." + "breakTorque".VariableHighlight());
            }
            else if (output == Unit.connectedBody)
            {
                return MakeSelectableForThisUnit("brokenJoint".VariableHighlight() + "." + "connectedBody".VariableHighlight());
            }
            else if (output == Unit.reactionForce)
            {
                return MakeSelectableForThisUnit("brokenJoint".VariableHighlight() + "." + "reactionForce".VariableHighlight());
            }
            else if (output == Unit.reactionTorque)
            {
                return MakeSelectableForThisUnit("brokenJoint".VariableHighlight() + "." + "reactionTorque".VariableHighlight());
            }
            else
            {
                return MakeSelectableForThisUnit("brokenJoint".VariableHighlight());
            }
        }
    }
}
#endif