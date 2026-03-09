using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(ColorString))]
    public class ColorStringGenerator : NodeGenerator<ColorString>
    {
        public ColorStringGenerator(Unit unit) : base(unit) { }
        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.CallCSharpUtilityMethod("ColorString", writer.Action(() =>
            {
                using (data.Expect(typeof(string)))
                    GenerateValue(Unit.Value, data, writer);
            }), writer.ObjectString(Unit.color));
        }
    }
}