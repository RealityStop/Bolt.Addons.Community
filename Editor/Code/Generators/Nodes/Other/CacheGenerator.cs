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

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.Write(data.GetVariableName(Name, true, $"Error finding variable {Name}").VariableHighlight());
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            Name = data.AddLocalNameInScope(Name);
            variableType = GetSourceType(Unit.input, data, writer);
            writer.WriteIndented("var ".ConstructHighlight() + Name.VariableHighlight()).Equal();
            GenerateValue(Unit.input, data, writer);
            writer.Write(";").NewLine();
            GenerateExitControl(Unit.exit, data, writer);
        }
    }
}
