using System;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(NegativeValueNode))]
    public class NegativeValueNodeGenerator : NodeGenerator<NegativeValueNode>
    {
        public NegativeValueNodeGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            var valueInput = Unit.type switch
            {
                NegateType.Float => Unit.Float,
                NegateType.Int => Unit.Int,
                NegateType.Vector2 => Unit.Vector2,
                NegateType.Vector3 => Unit.Vector3,
                _ => throw new NotSupportedException($"Unsupported type: {Unit.type}")
            };
            return MakeClickableForThisUnit("-") + GenerateValue(valueInput, data);
        }
    }
}
