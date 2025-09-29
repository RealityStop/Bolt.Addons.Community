namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(DecrementNode))]
    public class DecrementNodeGenerator : BaseIncrementDecrementNodeGenerator<DecrementNode>
    {
        public DecrementNodeGenerator(Unit unit) : base(unit)
        {
        }

        protected override string OperationKeyword => "--";

        protected override string MethodName => "Decrement";
    }
}