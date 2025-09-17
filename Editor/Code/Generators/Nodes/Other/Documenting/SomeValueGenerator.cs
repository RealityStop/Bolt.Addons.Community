using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(SomeValue))]
    public class SomeValueGenerator : NodeGenerator<SomeValue>
    {
        private object value;
        public SomeValueGenerator(Unit unit) : base(unit)
        {
            if (Unit.IsInteger)
                value = UnityEngine.Random.Range(0, 1);
            else
                value = UnityEngine.Random.Range(0.0f, 1.0f);
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return MakeClickableForThisUnit(value.As().Code(false));
        }
    }
}