using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Null))]
    public class NullGenerator : NodeGenerator<Null>
    {
        public NullGenerator(Unit unit) : base(unit)
        {
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            var expected = data.GetExpectedType();
            if (expected != null && expected.IsReferenceType())
            {
                data.MarkExpectedTypeMet(typeof(Null));
            }
            writer.Null();
        }
    }
}