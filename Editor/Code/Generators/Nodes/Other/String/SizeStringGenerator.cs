using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(SizeString))]
    public class SizeStringGenerator : NodeGenerator<SizeString>
    {
        public SizeStringGenerator(Unit unit) : base(unit) { }
        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.CallCSharpUtilityMethod("SizeString", writer.Action(() =>
            {
                using (data.Expect(typeof(string)))
                    GenerateValue(Unit.Value, data, writer);
            }), writer.ObjectString(Unit.size));
        }
    }
}