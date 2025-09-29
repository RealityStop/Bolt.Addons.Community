using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    public abstract class BaseRoundGenerator<TInput, TOutput> : NodeGenerator<Round<TInput, TOutput>>
    {
        public BaseRoundGenerator(Unit unit) : base(unit) { NameSpaces = $"{"static".ConstructHighlight()} Unity.VisualScripting.{"Round".TypeHighlight()}<{"float".ConstructHighlight()}` {"float".ConstructHighlight()}>"; }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            data.SetExpectedType(typeof(TInput));
            string input = GenerateValue(Unit.input, data);
            data.RemoveExpectedType();
            return CodeBuilder.CallCSharpUtilityMethod(Unit, MakeClickableForThisUnit("Round"), input, Unit.rounding.As().Code(false, Unit));
        }
    }
}