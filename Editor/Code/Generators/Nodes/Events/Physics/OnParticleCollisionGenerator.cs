using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Utility;
using UnityEngine;

#if MODULE_PHYSICS_EXISTS
namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnParticleCollision))]
    public class OnParticleCollisionGenerator : UnityMethodGenerator<OnParticleCollision, GameObject>
    {
        public OnParticleCollisionGenerator(Unit unit) : base(unit)
        {
        }

        public override List<ValueOutput> OutputValues => new List<ValueOutput>() { Unit.other, Unit.collisionEvents };

        public override List<TypeParam> Parameters => new List<TypeParam>() { new TypeParam(typeof(GameObject), "other") };
        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (Unit.collisionEvents.hasValidConnection)
            {
                writer.CreateVariable(typeof(List<ParticleCollisionEvent>), "collisionEvents", writer.Action(() =>
                {
                    writer.New(typeof(List<ParticleCollisionEvent>));
                }));
                writer.NewLine();
            }

            data.AddLocalNameInScope("collisionEvents", typeof(List<ParticleCollisionEvent>));
            GenerateChildControl(Unit.trigger, data, writer);
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (output == Unit.other)
            {
                writer.GetVariable("other");
            }
            else
            {
                writer.InvokeMember(writer.Action(() => GenerateValue(Unit.target, data, writer)), "GetComponent", 
                new CodeWriter.TypeParameter[] { typeof(ParticleSystem) }).InvokeMember(null, "GetCollisionEvents", 
                "other".VariableHighlight(), "collisionEvents".VariableHighlight());
            }
        }
    }
}
#endif