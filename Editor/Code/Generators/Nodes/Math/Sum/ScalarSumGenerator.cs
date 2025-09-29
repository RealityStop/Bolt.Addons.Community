namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(ScalarSum))]
    public class ScalarSumGenerator : SumGenerator<ScalarSum>
    {
        public ScalarSumGenerator(Unit unit) : base(unit)
        {
        }
    }
}
