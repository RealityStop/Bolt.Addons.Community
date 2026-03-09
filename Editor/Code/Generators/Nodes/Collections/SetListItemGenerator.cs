namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(SetListItem))]
    public class SetListItemGenerator : NodeGenerator<SetListItem>
    {
        public SetListItemGenerator(Unit unit) : base(unit)
        {
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            writer.WriteIndented();
            GenerateValue(Unit.list, data, writer);
            writer.Brackets(w =>
            {
                GenerateValue(Unit.index, data, writer);
            });

            writer.Equal();

            GenerateValue(Unit.item, data, writer);

            writer.WriteEnd(EndWriteOptions.LineEnd);

            GenerateExitControl(Unit.exit, data, writer);
        }
    }
}