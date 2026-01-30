namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(SetDictionaryItem))]
    public class SetDictionaryItemGenerator : NodeGenerator<SetDictionaryItem>
    {
        public SetDictionaryItemGenerator(Unit unit) : base(unit) { }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            writer.WriteIndented();
            GenerateValue(Unit.dictionary, data, writer);
            writer.Brackets(w =>
            {
                GenerateValue(Unit.key, data, writer);
            });

            writer.Equal();

            GenerateValue(Unit.value, data, writer);

            writer.WriteEnd(EndWriteOptions.LineEnd);

            GenerateExitControl(Unit.exit, data, writer);
        }
    }
}
