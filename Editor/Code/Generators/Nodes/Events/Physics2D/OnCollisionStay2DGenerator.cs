using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Utility;
using UnityEngine;

#if MODULE_PHYSICS_2D_EXISTS
namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnCollisionStay2D))]
    public class OnCollisionStay2DGenerator : UnityMethodGenerator<OnCollisionStay2D, Collision2D>
    {
        public OnCollisionStay2DGenerator(Unit unit) : base(unit)
        {
        }

        public override List<ValueOutput> OutputValues => new List<ValueOutput>() { Unit.collider, Unit.contacts, Unit.relativeVelocity, Unit.enabled, Unit.data };

        public override List<TypeParam> Parameters => new List<TypeParam>() { new TypeParam(typeof(Collision2D), "other") };


        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (output == Unit.collider)
            {
                writer.GetVariable("other").GetMember("collider");
            }
            else if (output == Unit.contacts)
            {
                writer.GetVariable("other").GetMember("contacts");
            }
            else if (output == Unit.relativeVelocity)
            {
                writer.GetVariable("other").GetMember("relativeVelocity");
            }
            else if (output == Unit.enabled)
            {
                writer.GetVariable("other").GetMember("enabled");
            }
            else
            {
                writer.GetVariable("other");
            }
        }
    }
}
#endif