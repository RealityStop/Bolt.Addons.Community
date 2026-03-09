using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(ClearDictionary))]
    public class ClearDictionaryGenerator : NodeGenerator<ClearDictionary>
    {
        public ClearDictionaryGenerator(Unit unit) : base(unit) { }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            using (data.Expect(typeof(System.Collections.IDictionary)))
            {
                writer.WriteIndented();
                GenerateValue(Unit.dictionaryInput, data, writer);
            }

            writer.InvokeMember(null, "Clear").WriteEnd(EndWriteOptions.LineEnd);

            GenerateExitControl(Unit.exit, data, writer);
        }
    }
}
