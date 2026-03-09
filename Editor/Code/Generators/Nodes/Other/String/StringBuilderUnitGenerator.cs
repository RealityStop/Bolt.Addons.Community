using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(StringBuilderUnit))]
    public class StringBuilderGenerator : NodeGenerator<StringBuilderUnit>
    {
        public StringBuilderGenerator(Unit unit) : base(unit) { }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            for (int i = 0; i < Unit.appendModes.Count; i++)
            {
                if (i != 0)
                {
                    writer.Write(" + ");
                }

                WriteAppendSegment(Unit.appendModes[i].appendMode, Unit.inputPorts[i], i, Unit.appendModes.Count, data, writer);
            }
        }

        private void WriteAppendSegment(StringBuilderUnit.AppendMode mode, ValueInput input, int index, int totalCount, ControlGenerationData data, CodeWriter writer)
        {
            switch (mode)
            {
                case StringBuilderUnit.AppendMode.UpperCase:
                    GenerateValue(input, data, writer);
                    writer.Write(".ToUpperInvariant()");
                    break;

                case StringBuilderUnit.AppendMode.LowerCase:
                    GenerateValue(input, data, writer);
                    writer.Write(".ToLowerInvariant()");
                    break;

                case StringBuilderUnit.AppendMode.Trimmed:
                    GenerateValue(input, data, writer);
                    writer.Write(".Trim()");
                    break;

                case StringBuilderUnit.AppendMode.Quoted:
                    writer.Write("\"\\\"\"".StringHighlight());
                    writer.Write(" + ");
                    GenerateValue(input, data, writer);
                    writer.Write(" + ");
                    writer.Write("\"\\\"\"".StringHighlight());
                    break;

                case StringBuilderUnit.AppendMode.Prefixed:
                    writer.Write($"\"{Unit.appendModes[index].prefix}\"".StringHighlight());
                    writer.Write(" + ");
                    GenerateValue(input, data, writer);
                    break;

                case StringBuilderUnit.AppendMode.Suffixed:
                    GenerateValue(input, data, writer);
                    writer.Write(" + ");
                    writer.Write($"\"{Unit.appendModes[index].suffix}\"".StringHighlight());
                    break;

                case StringBuilderUnit.AppendMode.Repeated:
                    int repeat = Unit.appendModes[index].repeatCount;
                    for (int i = 0; i < repeat; i++)
                    {
                        if (i != 0)
                            writer.Write(" + ");

                        GenerateValue(input, data, writer);
                    }
                    break;

                case StringBuilderUnit.AppendMode.TabBefore:
                    writer.Write("\"\\t\"".StringHighlight());
                    writer.Write(" + ");
                    GenerateValue(input, data, writer);
                    break;

                case StringBuilderUnit.AppendMode.TabAfter:
                    GenerateValue(input, data, writer);
                    writer.Write(" + ");
                    writer.Write("\"\\t\"".StringHighlight());
                    break;

                case StringBuilderUnit.AppendMode.SpaceBefore:
                    writer.Write("\" \"".StringHighlight());
                    writer.Write(" + ");
                    GenerateValue(input, data, writer);
                    break;

                case StringBuilderUnit.AppendMode.SpaceAfter:
                    GenerateValue(input, data, writer);
                    writer.Write(" + ");
                    writer.Write("\" \"".StringHighlight());
                    break;

                case StringBuilderUnit.AppendMode.NewLineBefore:
                    writer.Write("\"\\n\"".StringHighlight());
                    writer.Write(" + ");
                    GenerateValue(input, data, writer);
                    break;

                case StringBuilderUnit.AppendMode.NewLineAfter:
                    GenerateValue(input, data, writer);
                    writer.Write(" + ");
                    writer.Write("\"\\n\"".StringHighlight());
                    break;

                case StringBuilderUnit.AppendMode.CommaSeparated:
                    GenerateValue(input, data, writer);
                    if (index < totalCount - 1)
                    {
                        writer.Write(" + ");
                        writer.Write("\", \"".StringHighlight());
                    }
                    break;

                case StringBuilderUnit.AppendMode.Delimiter:
                    GenerateValue(input, data, writer);
                    if (index < totalCount - 1)
                    {
                        writer.Write(" + ");
                        writer.Write($"\"{Unit.appendModes[index].delimiter}\"".StringHighlight());
                    }
                    break;

                case StringBuilderUnit.AppendMode.Default:
                default:
                    GenerateValue(input, data, writer);
                    break;
            }
        }
    }
}