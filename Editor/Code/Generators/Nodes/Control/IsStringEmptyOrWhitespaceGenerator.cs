using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(IsStringEmptyOrWhitespace))]
    public sealed class IsStringEmptyOrWhitespaceGenerator : NodeGenerator<IsStringEmptyOrWhitespace>
    {
        public IsStringEmptyOrWhitespaceGenerator(IsStringEmptyOrWhitespace unit) : base(unit) { }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input != Unit.Input)
                return;

            writer.WriteIndented("if ".ControlHighlight() + "(");
            writer.InvokeMember(typeof(string), "IsNullOrWhiteSpace", writer.Action(() =>
            {
                GenerateValue(Unit.String, data, writer);
            }));
            writer.WriteEnd(EndWriteOptions.CloseParentheses | EndWriteOptions.Newline);

            writer.WriteLine("{");

            using (writer.IndentedScope(data))
            {
                GenerateChildControl(Unit.True, data, writer);
            }

            writer.WriteLine("}");

            if (!Unit.False.hasAnyConnection)
            {
                return;
            }

            writer.WriteLine("else".ControlHighlight());

            writer.WriteLine("{");

            using (writer.IndentedScope(data))
            {
                GenerateChildControl(Unit.False, data, writer);
            }

            writer.WriteLine("}");
        }

        protected override void GenerateValueInternal(ValueInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input == Unit.String && !input.hasValidConnection && Unit.defaultValues[input.key] == null)
            {
                writer.Object("");
                return;
            }

            base.GenerateValueInternal(input, data, writer);
        }
    }
}