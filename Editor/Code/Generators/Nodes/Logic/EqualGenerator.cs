using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Equal))]
    public sealed class EqualGenerator : BinaryComparisonGenerator<Equal>
    {
        public EqualGenerator(Equal unit) : base(unit) { }
        protected override ValueInput LeftInput => Unit.a;
        protected override ValueInput RightInput => Unit.b;
        protected override ValueOutput OutputPort => Unit.comparison;
        protected override string OperatorToken => " == ";
        protected override string OperatorMethodName => "op_Equality";
    }
}