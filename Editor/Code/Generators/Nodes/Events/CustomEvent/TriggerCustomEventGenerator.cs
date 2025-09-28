using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community 
{
    [NodeGenerator(typeof(TriggerCustomEvent))]
    public sealed class TriggerCustomEventGenerator : NodeGenerator<TriggerCustomEvent>
    {
        public TriggerCustomEventGenerator(Unit unit) : base(unit)
        {
        }
    
        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;
            var customEvent = typeof(CustomEvent).As().CSharpName(false, true);
    
            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit(customEvent + ".Trigger(") + GenerateValue(Unit.target, data) + MakeClickableForThisUnit(", ") + $"{GenerateValue(Unit.name, data)}{(Unit.argumentCount > 0 ? MakeClickableForThisUnit(", ") : "")}{string.Join(MakeClickableForThisUnit(", "), Unit.arguments.Select(arg => GenerateValue(arg, data)))}" + MakeClickableForThisUnit(");") + "\n";
            output += GetNextUnit(Unit.exit, data, indent);
            return output;
        }
    
        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input == Unit.target && !Unit.target.hasValidConnection)
            {
                return MakeClickableForThisUnit("gameObject".VariableHighlight());
            }
    
            if (input == Unit.target)
            {
                var sourceType = GetSourceType(Unit.target, data);
                var sourceIsComponent = typeof(Component).IsAssignableFrom(sourceType);
                if (sourceIsComponent)
                {
                    return base.GenerateValue(Unit.target, data) + MakeClickableForThisUnit("." + "gameObject".VariableHighlight());
                }
                else
                {
                    data.SetExpectedType(typeof(GameObject));
                    var code = base.GenerateValue(Unit.target, data);
                    data.RemoveExpectedType();
                    return code;
                }
            }
            else if (Unit.arguments.Contains(input))
            {
                data.SetExpectedType(typeof(object));
                var code = base.GenerateValue(input, data);
                data.RemoveExpectedType();
                return code;
            }
            return base.GenerateValue(input, data);
        }
    } 
}