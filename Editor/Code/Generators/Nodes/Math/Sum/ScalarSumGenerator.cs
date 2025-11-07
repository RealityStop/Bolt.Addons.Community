namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(ScalarSum))]
    public class ScalarSumGenerator : SumGenerator<ScalarSum>
    {
        public ScalarSumGenerator(Unit unit) : base(unit)
        {
        }
    }
}
