namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(RemoveListItemAt))]
    public class RemoveListItemAtGenerator : NodeGenerator<RemoveListItemAt>
    {
        public RemoveListItemAtGenerator(Unit unit) : base(unit) { }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            using (data.Expect(Unit.listInput.type))
            {
                writer.WriteIndented();
                GenerateValue(Unit.listInput, data, writer);
            }

            writer.InvokeMember(null, "RemoveAt", writer.Action(() => GenerateValue(Unit.index, data, writer)));

            writer.WriteEnd(EndWriteOptions.LineEnd);

            GenerateExitControl(Unit.exit, data, writer);
        }
    }
}