using Unity.VisualScripting;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(StringBuilderUnit))]
    public class StringBuilderGenerator : NodeGenerator<StringBuilderUnit>
    {
        public StringBuilderGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            List<string> codeSegments = new List<string>();

            // Iterate through the input ports and generate the appropriate concatenations.
            for (int i = 0; i < this.Unit.appendModes.Count; i++)
            {
                var mode = this.Unit.appendModes[i].appendMode;
                string inputValue = GenerateValue(Unit.inputPorts[i], data);
                string segment = HandleAppendMode(mode, inputValue, i, this.Unit.appendModes.Count);

                codeSegments.Add(segment);
            }

            // Concatenate all segments into one final string expression.
            return string.Join(MakeClickableForThisUnit(" + "), codeSegments);
        }

        /// <summary>
        /// Generates the appropriate string concatenation based on the given mode.
        /// </summary>
        private string HandleAppendMode(StringBuilderUnit.AppendMode mode, string inputValue, int index, int totalCount)
        {
            switch (mode)
            {
                case StringBuilderUnit.AppendMode.UpperCase:
                    return inputValue + MakeClickableForThisUnit(".ToUpper()");

                case StringBuilderUnit.AppendMode.LowerCase:
                    return inputValue + MakeClickableForThisUnit(".ToLower()");
                case StringBuilderUnit.AppendMode.Quoted:
                    return MakeClickableForThisUnit($"{"\"\\\"\"".StringHighlight()} + ") + inputValue + MakeClickableForThisUnit($" + {"\"\\\"\"".StringHighlight()}");

                case StringBuilderUnit.AppendMode.Trimmed:
                    return inputValue + MakeClickableForThisUnit(".Trim()");

                case StringBuilderUnit.AppendMode.Prefixed:
                    string prefix = this.Unit.appendModes[index].prefix;
                    return MakeClickableForThisUnit($"\"{prefix}\"".StringHighlight()) + MakeClickableForThisUnit(" + ") + inputValue;

                case StringBuilderUnit.AppendMode.Suffixed:
                    string suffix = this.Unit.appendModes[index].suffix;
                    return inputValue + MakeClickableForThisUnit(" + ") + MakeClickableForThisUnit($"\"{suffix}\"".StringHighlight());

                case StringBuilderUnit.AppendMode.Repeated:
                    int repetitions = this.Unit.appendModes[index].repeatCount;
                    List<string> output = new List<string>();
                    for (int i = 0; i < repetitions; i++) output.Add(inputValue);
                    return string.Join(MakeClickableForThisUnit(" + "), output);
                case StringBuilderUnit.AppendMode.TabAfter:
                    return inputValue + MakeClickableForThisUnit($" + {"\"\\t\"".StringHighlight()}");
                case StringBuilderUnit.AppendMode.TabBefore:
                    return MakeClickableForThisUnit($"{"\"\\t\"".StringHighlight()} + ") + inputValue;
                case StringBuilderUnit.AppendMode.CommaSeparated:
                    if (index < totalCount - 1)
                        return inputValue + MakeClickableForThisUnit($" + {"\", \"".StringHighlight()}");
                    return inputValue;
                case StringBuilderUnit.AppendMode.SpaceAfter:
                    return inputValue + MakeClickableForThisUnit($" + {"\" \"".StringHighlight()}");
                case StringBuilderUnit.AppendMode.SpaceBefore:
                    return MakeClickableForThisUnit($"{"\" \"".StringHighlight()} + ") + inputValue;
                case StringBuilderUnit.AppendMode.NewLineBefore:
                    return MakeClickableForThisUnit($"{"\"\\n\"".StringHighlight()} + ") + inputValue;
                case StringBuilderUnit.AppendMode.NewLineAfter:
                    return inputValue + MakeClickableForThisUnit($" + {"\"\\n\"".StringHighlight()}");
                case StringBuilderUnit.AppendMode.Delimiter:
                    string delimiter = this.Unit.appendModes[index].delimiter;
                    if (index < totalCount - 1)
                        return inputValue + MakeClickableForThisUnit($" + {$"\"{delimiter}\"".StringHighlight()}");
                    return inputValue;

                case StringBuilderUnit.AppendMode.Default:
                default:
                    return inputValue;
            }
        }
    }
}