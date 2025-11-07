using Unity;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
using System.Reflection;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(SelectExpose))]
    public sealed class SelectExposeGenerator : NodeGenerator<SelectExpose>
    {
        public SelectExposeGenerator(SelectExpose unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            var members = Unit.type.GetMembers().Where(m => m is FieldInfo || m is PropertyInfo).Select(m => m.ToManipulator(Unit.type)).DistinctBy(m => m.name).Where(Unit.Include).Where(m => Unit.selectedMembers.Contains(m.name));

            var member = members.FirstOrDefault(m => m.name == output.key);

            if (member != null)
            {
                if (Unit.instance && member.requiresTarget)
                {
                    return GenerateValue(Unit.target, data) + MakeClickableForThisUnit("." + output.key.VariableHighlight());
                }
                else if (Unit.@static && !member.requiresTarget)
                {
                    return MakeClickableForThisUnit(Unit.type.As().CSharpName(false, true) + "." + output.key.VariableHighlight());
                }
            }

            return base.GenerateValue(output, data);
        }

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input == Unit.target)
            {
                if (input.nullMeansSelf && !input.hasValidConnection)
                {
                    if (input.type == typeof(GameObject))
                    {
                        return MakeClickableForThisUnit("gameObject".VariableHighlight());
                    }
                    else if (typeof(Component).IsAssignableFrom(input.type))
                    {
                        return MakeClickableForThisUnit("gameObject".VariableHighlight() + ".GetComponent<" + input.type.As().CSharpName(false, true) + ">()");
                    }
                }
            }
            return base.GenerateValue(input, data);
        }
    }
}