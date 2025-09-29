using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Null))]
    public class NullGenerator : NodeGenerator<Null>
    {
        public NullGenerator(Unit unit) : base(unit)
        {
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {

            return MakeClickableForThisUnit("null".ConstructHighlight());
        }
    }

}