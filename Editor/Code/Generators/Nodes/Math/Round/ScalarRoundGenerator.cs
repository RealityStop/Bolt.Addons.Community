using Unity.VisualScripting;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(ScalarRound))]
    public class ScalarRoundGenerator : BaseRoundGenerator<float, int>
    {
        public ScalarRoundGenerator(Unit unit) : base(unit) { }
    }
}