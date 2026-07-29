namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Greater))]
    public sealed class GreaterGenerator : BinaryComparisonGenerator<Greater>
    {
        public GreaterGenerator(Greater unit) : base(unit) { }
        protected override ValueInput LeftInput => Unit.a;
        protected override ValueInput RightInput => Unit.b;
        protected override ValueOutput OutputPort => Unit.comparison;
        protected override string OperatorToken => " > ";
        protected override string OperatorMethodName => "op_GreaterThan";
    }
}