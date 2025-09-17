using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community 
{
    public abstract class BaseAverageGenerator<T> : NodeGenerator<Average<T>>
    {
        protected BaseAverageGenerator(Unit unit) : base(unit)
        {
        }
    
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            List<string> values = new();
            foreach (var input in Unit.multiInputs)
            {
                data.SetExpectedType(typeof(T));
                values.Add(GenerateValue(input, data));
                data.RemoveExpectedType();
            }
            return CodeBuilder.CallCSharpUtilityMethod(Unit, MakeClickableForThisUnit("CalculateAverage"), string.Join(MakeClickableForThisUnit(", "), values));
        }
    } 
}