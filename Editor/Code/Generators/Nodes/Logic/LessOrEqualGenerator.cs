namespace Unity.VisualScripting.Community.CSharp
{
[NodeGenerator(typeof(LessOrEqual))]
    public sealed class LessOrEqualGenerator : BinaryComparisonGenerator<LessOrEqual>
    {
        public LessOrEqualGenerator(LessOrEqual unit) : base(unit) { }
        protected override ValueInput LeftInput => Unit.a;
        protected override ValueInput RightInput => Unit.b;
        protected override ValueOutput OutputPort => Unit.comparison;
        protected override string OperatorToken => " <= ";
        protected override string OperatorMethodName => "op_LessThanOrEqual";
    }
}