namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(And))]
    public sealed class AndGenerator : LogicalOperatorGenerator<And>
    {
        public AndGenerator(And unit) : base(unit) { }
        protected override ValueInput LeftInput => Unit.a;
        protected override ValueInput RightInput => Unit.b;
        protected override ValueOutput OutputPort => Unit.result;
        protected override string OperatorToken => " && ";
    }
}