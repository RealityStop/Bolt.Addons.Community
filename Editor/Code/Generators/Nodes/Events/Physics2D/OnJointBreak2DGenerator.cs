using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Utility;
using UnityEngine;

#if MODULE_PHYSICS_EXISTS
namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnJointBreak2D))]
    public class OnJointBreak2DGenerator : UnityMethodGenerator<OnJointBreak2D, Joint2D>
    {
        public OnJointBreak2DGenerator(Unit unit) : base(unit)
        {
        }

        public override List<ValueOutput> OutputValues => new List<ValueOutput>() { Unit.breakForce, Unit.breakTorque, Unit.connectedBody, Unit.reactionForce, Unit.reactionTorque, Unit.joint };

        public override List<TypeParam> Parameters => new List<TypeParam>() { new TypeParam(typeof(Joint2D), "brokenJoint") };


        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (output == Unit.breakForce)
            {
                writer.GetVariable("brokenJoint").GetMember("breakForce");
            }
            else if (output == Unit.breakTorque)
            {
                writer.GetVariable("brokenJoint").GetMember("breakTorque");
            }
            else if (output == Unit.connectedBody)
            {
                writer.GetVariable("brokenJoint").GetMember("connectedBody");
            }
            else if (output == Unit.reactionForce)
            {
                writer.GetVariable("brokenJoint").GetMember("reactionForce");
            }
            else if (output == Unit.reactionTorque)
            {
                writer.GetVariable("brokenJoint").GetMember("reactionTorque");
            }
            else
            {
                writer.GetVariable("brokenJoint");
            }
        }
    }
}
#endif