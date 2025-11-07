using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    public abstract class PerSecondGenerator<T> : NodeGenerator<PerSecond<T>>
    {
        public PerSecondGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return GenerateValue(Unit.input, data) + MakeClickableForThisUnit(" * " + "Time".TypeHighlight() + "." + "deltaTime".VariableHighlight());
        }
    }
}
