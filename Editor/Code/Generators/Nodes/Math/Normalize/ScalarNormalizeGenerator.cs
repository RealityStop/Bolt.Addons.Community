using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(ScalarNormalize))]
    public class ScalarNormalizeGenerator : BaseNormalizeGenerator<float>
    {
        public ScalarNormalizeGenerator(Unit unit) : base(unit) { }
    }
}
