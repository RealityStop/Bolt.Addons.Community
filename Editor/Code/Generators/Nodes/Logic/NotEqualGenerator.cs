namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(NotEqual))]
    public sealed class NotEqualGenerator : BinaryComparisonGenerator<NotEqual>
    {
        public NotEqualGenerator(NotEqual unit) : base(unit) { }
        protected override ValueInput LeftInput => Unit.a;
        protected override ValueInput RightInput => Unit.b;
        protected override ValueOutput OutputPort => Unit.comparison;
        protected override string OperatorToken => " != ";
        protected override string OperatorMethodName => "op_Inequality";
    }
}