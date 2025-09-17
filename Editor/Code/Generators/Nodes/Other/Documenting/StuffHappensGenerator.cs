
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(StuffHappens))]
    public class StuffHappensGenerator : NodeGenerator<StuffHappens>
    {
        
        public StuffHappensGenerator(Unit unit) : base(unit)
        {
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return base.GenerateValue(output, data);
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return MakeClickableForThisUnit("// Stuff happens".CommentHighlight()) + "\n " + GetNextUnit(Unit.exit, data, indent);
        }
    }
}
