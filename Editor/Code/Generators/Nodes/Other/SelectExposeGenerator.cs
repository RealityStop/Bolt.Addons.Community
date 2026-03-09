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

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            var members = Unit.type.GetMembers().Where(m => m is FieldInfo || m is PropertyInfo).Select(m => m.ToManipulator(Unit.type)).DistinctBy(m => m.name).Where(Unit.Include).Where(m => Unit.selectedMembers.Contains(m.name));

            var member = members.FirstOrDefault(m => m.name == output.key);

            if (member != null)
            {
                if (Unit.instance && member.requiresTarget)
                {
                    GenerateValue(Unit.target, data, writer);
                    writer.Write("." + output.key.VariableHighlight());
                }
                else if (Unit.@static && !member.requiresTarget)
                {
                    writer.Write(Unit.type.As().CSharpName(false, true) + "." + output.key.VariableHighlight());
                }
            }
        }
    }
}