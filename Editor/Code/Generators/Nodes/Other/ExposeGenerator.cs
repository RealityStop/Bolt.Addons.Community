using Unity;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Unity.VisualScripting.Expose))]
    public sealed class ExposeGenerator : NodeGenerator<Unity.VisualScripting.Expose>
    {
        public ExposeGenerator(Unity.VisualScripting.Expose unit) : base(unit)
        {
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            GenerateValue(Unit.target, data, writer);
            writer.Write("." + output.key.VariableHighlight());
        }
    }
}