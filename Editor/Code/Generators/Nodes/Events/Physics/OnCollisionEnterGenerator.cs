using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Utility;
using UnityEngine;

#if MODULE_PHYSICS_EXISTS
namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnCollisionEnter))]
    public class OnCollisionEnterGenerator : UnityMethodGenerator<OnCollisionEnter, Collision>
    {
        public OnCollisionEnterGenerator(Unit unit) : base(unit)
        {
        }

        public override List<ValueOutput> OutputValues => new List<ValueOutput>() { Unit.collider, Unit.contacts, Unit.relativeVelocity, Unit.impulse, Unit.data };

        public override List<TypeParam> Parameters => new List<TypeParam>() { new TypeParam(typeof(Collision), "other") };

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
            else if (output == Unit.impulse)
            {
                writer.GetVariable("other").GetMember("impulse");
            }
            else
            {
                writer.GetVariable("other");
            }
        }
    }
}
#endif