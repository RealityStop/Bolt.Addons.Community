using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(MultilineStringNode))]
    public class MultilineStringNodeGenerator : NodeGenerator<MultilineStringNode>
    {
        public MultilineStringNodeGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return Unit.stringLiteral.As().Code(true, unit, true, true, "", true, true);
        }
    }
}
