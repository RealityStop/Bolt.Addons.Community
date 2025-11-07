using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Cache))]
    public class CacheGenerator : LocalVariableGenerator
    {
        private Cache Unit => unit as Cache;

        private string Name = "cachedValue";

        public CacheGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            
            return MakeClickableForThisUnit(data.GetVariableName(Name, true, CodeUtility.ErrorTooltip($"The variable '{Name}' could not be found, it could be because you are trying to access this value across different flows.", $"Error finding variable {Name}", "")).VariableHighlight());
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;
            Name = data.AddLocalNameInScope(Name);
            variableType = GetSourceType(Unit.input, data);
            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("var ".ConstructHighlight() + Name.VariableHighlight() + " = ") + GenerateValue(Unit.input, data) + MakeClickableForThisUnit(";") + "\n";
            output += GetNextUnit(Unit.exit, data, indent);
            return output;
        }
    }
}
