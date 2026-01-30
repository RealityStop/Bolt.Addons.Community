using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(RemoveListItem))]
    public sealed class RemoveListItemGenerator : NodeGenerator<RemoveListItem>
    {
        public RemoveListItemGenerator(Unit unit) : base(unit) { }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            using (data.Expect(Unit.listInput.type))
            {
                writer.WriteIndented();
                GenerateValue(Unit.listInput, data, writer);
            }

            writer.InvokeMember(null, "Remove",writer.Action(() => GenerateValue(Unit.item, data, writer)));

            writer.WriteEnd(EndWriteOptions.LineEnd);

            GenerateExitControl(Unit.exit, data, writer);
        }
    }
}
