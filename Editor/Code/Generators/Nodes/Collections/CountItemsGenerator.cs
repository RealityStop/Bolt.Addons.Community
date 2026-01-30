using System.Collections;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(CountItems))]
    public class CountItemsGenerator : NodeGenerator<CountItems>
    {
        public CountItemsGenerator(Unit unit) : base(unit) { }

        public override IEnumerable<string> GetNamespaces()
        {
            yield return "System.Collections";
            yield return "System.Linq";
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            GenerateValue(Unit.collection, data, writer);

            var type = GetSourceType(Unit.collection, data, writer, false);

            if (type != null && (type.IsArray || type == typeof(string)))
            {
                writer.GetMember(null, "Length");
                return;
            }
            else if (typeof(ICollection).IsStrictlyAssignableFrom(type))
            {
                writer.GetMember(null, "Count");
                return;
            }

            writer.InvokeMember(null, "Cast", new CodeWriter.TypeParameter[] { typeof(object) })
            .InvokeMember(null, "Count");
        }
    }
}