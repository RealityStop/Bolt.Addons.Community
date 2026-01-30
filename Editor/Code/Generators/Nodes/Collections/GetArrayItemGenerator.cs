namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(GetArrayItem))]
    public class GetArrayItemGenerator : NodeGenerator<GetArrayItem>
    {
        public GetArrayItemGenerator(Unit unit) : base(unit) { }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            GenerateValue(Unit.array, data, writer);

            writer.Write("[");

            for (int i = 0; i < Unit.indexes.Count; i++)
            {
                GenerateValue(Unit.indexes[i], data, writer);

                if (i < Unit.indexes.Count - 1)
                {
                    writer.Write(", ");
                }
            }

            writer.Write("]");
        }
    }
}