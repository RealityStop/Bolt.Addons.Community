namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Less))]
    public sealed class LessGenerator : BinaryComparisonGenerator<Less>
    {
        public LessGenerator(Less unit) : base(unit) { }
        protected override ValueInput LeftInput => Unit.a;
        protected override ValueInput RightInput => Unit.b;
        protected override ValueOutput OutputPort => Unit.comparison;
        protected override string OperatorToken => " < ";
        protected override string OperatorMethodName => "op_LessThan";
    }
}