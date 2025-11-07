using Unity.VisualScripting;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(ScalarNormalize))]
    public class ScalarNormalizeGenerator : BaseNormalizeGenerator<float>
    {
        public ScalarNormalizeGenerator(Unit unit) : base(unit) { }
    }
}
