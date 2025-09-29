namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(GenericSum))]
    public class GenericSumGenerator : SumGenerator<GenericSum>
    {
        public GenericSumGenerator(Unit unit) : base(unit)
        {
        }
    }
}