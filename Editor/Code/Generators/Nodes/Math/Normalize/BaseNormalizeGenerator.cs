using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    public abstract class BaseNormalizeGenerator<T> : NodeGenerator<Normalize<T>>
    {
        public BaseNormalizeGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            data.SetExpectedType(typeof(T));
            string input = GenerateValue(Unit.input, data);
            data.RemoveExpectedType();

            return CodeBuilder.CallCSharpUtilityMethod(Unit, MakeClickableForThisUnit("Normalize"), input);
        }
    }
}
