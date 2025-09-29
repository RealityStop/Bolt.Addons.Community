using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(ScalarSubtract))]
    public class SubtractScalarGenerator : SubtractGenerator<float>
    {
        public SubtractScalarGenerator(Unit unit) : base(unit) { }
    }
}
