using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Utility;
using UnityEngine;

#if MODULE_PHYSICS_EXISTS
namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnControllerColliderHit))]
    public class OnControllerColliderHitGenerator : UnityMethodGenerator<OnControllerColliderHit, ControllerColliderHit>
    {
        public OnControllerColliderHitGenerator(Unit unit) : base(unit)
        {
        }

        public override List<ValueOutput> OutputValues => new List<ValueOutput>() { Unit.collider, Unit.controller, Unit.moveDirection, Unit.moveLength, Unit.normal, Unit.point, Unit.data };

        public override List<TypeParam> Parameters => new List<TypeParam>() { new TypeParam(typeof(ControllerColliderHit), "hit") };


        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (output == Unit.collider)
            {
                writer.GetVariable("hit").GetMember("collider");
            }
            else if (output == Unit.controller)
            {
                writer.GetVariable("hit").GetMember("controller");
            }
            else if (output == Unit.moveDirection)
            {
                writer.GetVariable("hit").GetMember("moveDirection");
            }
            else if (output == Unit.moveLength)
            {
                writer.GetVariable("hit").GetMember("moveLength");
            }
            else if (output == Unit.normal)
            {
                writer.GetVariable("hit").GetMember("normal");
            }
            else if (output == Unit.point)
            {
                writer.GetVariable("hit").GetMember("point");
            }
            else
            {
                writer.GetVariable("hit");
            }
        }
    }
}
#endif