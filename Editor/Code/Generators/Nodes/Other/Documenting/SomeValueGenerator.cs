using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
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

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.Object(value);
        }
    }
}