namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(ExclusiveOr))]
    public sealed class ExclusiveOrGenerator : BinaryComparisonGenerator<ExclusiveOr>
    {
        public ExclusiveOrGenerator(ExclusiveOr unit) : base(unit)
        {
        }

        protected override ValueInput LeftInput => Unit.a;
        protected override ValueInput RightInput => Unit.b;
        protected override ValueOutput OutputPort => Unit.result;
        protected override string OperatorToken => " ^ ";
        protected override string OperatorMethodName => "op_ExclusiveOr";
    }
}