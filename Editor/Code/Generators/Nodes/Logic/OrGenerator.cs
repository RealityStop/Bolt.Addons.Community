using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Or))]
    public sealed class OrGenerator : LogicalOperatorGenerator<Or>
    {
        public OrGenerator(Or unit) : base(unit) { }
        protected override ValueInput LeftInput => Unit.a;
        protected override ValueInput RightInput => Unit.b;
        protected override ValueOutput OutputPort => Unit.result;
        protected override string OperatorToken => " || ";
    }
}