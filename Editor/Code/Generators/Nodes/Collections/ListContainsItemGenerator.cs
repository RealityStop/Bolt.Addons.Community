using System;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(ListContainsItem))]
    public class ListContainsItemGenerator : NodeGenerator<ListContainsItem>
    {
        public ListContainsItemGenerator(Unit unit) : base(unit)
        {
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            using (data.Expect(Unit.list.type))
            {
                GenerateValue(Unit.list, data, writer);
            }
            writer.InvokeMember(null, "Contains", writer.Action(() => GenerateValue(Unit.item, data, writer)));
        }
    }
}