using Unity.VisualScripting;
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(AddListItem))]
    public class AddListItemGenerator : NodeGenerator<AddListItem>
    {
        public AddListItemGenerator(Unit unit) : base(unit)
        {
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            using (data.Expect(Unit.listInput.type))
            {
                writer.WriteIndented();
                GenerateValue(Unit.listInput, data, writer);
            }
            writer.Write(".Add(");

            GenerateValue(Unit.item, data, writer);

            writer.WriteEnd();

            GenerateExitControl(Unit.exit, data, writer);
        }
    }
}