namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(GenericSum))]
    public class GenericSumGenerator : SumGenerator<GenericSum>
    {
        public GenericSumGenerator(Unit unit) : base(unit)
        {
        }
    }
}