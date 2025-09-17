using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Utility;
using UnityEngine;

#if MODULE_PHYSICS_EXISTS
namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnParticleCollision))]
    public class OnParticleCollisionGenerator : UnityMethodGenerator<OnParticleCollision, GameObject>
    {
        public OnParticleCollisionGenerator(Unit unit) : base(unit)
        {
        }

        public override List<ValueOutput> OutputValues => new List<ValueOutput>() { Unit.other, Unit.collisionEvents };

        public override List<TypeParam> Parameters => new List<TypeParam>() { new TypeParam(typeof(GameObject), "other") };

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            if (!typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType)) return MakeClickableForThisUnit(CodeUtility.ToolTip($"{Community.NameUtility.DisplayName(typeof(OnParticleCollision))} only works with ScriptGraphAssets, ScriptMachines or a ClassAsset that inherits MonoBehaviour", $"Could not generate {Community.NameUtility.DisplayName(typeof(OnParticleCollision))}", ""));
            var body = Unit.collisionEvents.hasValidConnection ? CodeBuilder.Indent(indent) + MakeClickableForThisUnit($"{"var".ConstructHighlight()} {"collisionEvents".VariableHighlight()} = {new List<ParticleCollisionEvent>().As().Code(true)};") + "\n" : "";
            data.AddLocalNameInScope("collisionEvents", typeof(List<ParticleCollisionEvent>));
            body += GetNextUnit(Unit.trigger, data, indent);
            return body;
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (output == Unit.other)
            {
                return MakeClickableForThisUnit("other".VariableHighlight());
            }
            else
            {
                return GenerateValue(Unit.target, data) + MakeClickableForThisUnit($".GetComponent<{"ParticleSystem".TypeHighlight()}>().GetCollisionEvents({"other".VariableHighlight()}, {"collisionEvents".VariableHighlight()})");
            }
        }

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input == Unit.target)
            {
                if (Unit.target.hasValidConnection)
                {
                    return base.GenerateValue(input, data);
                }
                else if (Unit.target.hasDefaultValue)
                {
                    var value = input.unit.defaultValues[input.key];
                    if (value == null)
                    {
                        return MakeClickableForThisUnit("gameObject".VariableHighlight());
                    }
                    else
                    {
                        return input.unit.defaultValues[input.key].As().Code(true, unit, true, true, "", true, true);
                    }
                }
            }
            return base.GenerateValue(input, data);
        }
    }
}
#endif