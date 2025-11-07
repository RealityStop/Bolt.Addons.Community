namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(IncrementNode))]
    public class IncrementNodeGenerator : BaseIncrementDecrementNodeGenerator<IncrementNode>
    {
        public IncrementNodeGenerator(Unit unit) : base(unit)
        {
        }

        protected override string OperationKeyword => "++";

        protected override string MethodName => "Increment";
    }
}