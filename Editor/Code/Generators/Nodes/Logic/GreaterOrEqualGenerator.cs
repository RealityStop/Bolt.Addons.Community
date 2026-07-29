namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(GreaterOrEqual))]
    public sealed class GreaterOrEqualGenerator : BinaryComparisonGenerator<GreaterOrEqual>
    {
        public GreaterOrEqualGenerator(GreaterOrEqual unit) : base(unit) { }
        protected override ValueInput LeftInput => Unit.a;
        protected override ValueInput RightInput => Unit.b;
        protected override ValueOutput OutputPort => Unit.comparison;
        protected override string OperatorToken => " >= ";
        protected override string OperatorMethodName => "op_GreaterThanOrEqual";
    }
}