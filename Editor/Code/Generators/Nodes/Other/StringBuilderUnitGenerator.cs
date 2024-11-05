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
            return string.Join(MakeSelectableForThisUnit(" + "), codeSegments);
        }

        /// <summary>
        /// Generates the appropriate string concatenation based on the given mode.
        /// </summary>
        private string HandleAppendMode(StringBuilderUnit.AppendMode mode, string inputValue, int index, int totalCount)
        {
            switch (mode)
            {
                case StringBuilderUnit.AppendMode.UpperCase:
                    return inputValue + MakeSelectableForThisUnit(".ToUpper()");

                case StringBuilderUnit.AppendMode.LowerCase:
                    return inputValue + MakeSelectableForThisUnit(".ToLower()");
                case StringBuilderUnit.AppendMode.Quoted:
                    return MakeSelectableForThisUnit($"{"\"\\\"\"".StringHighlight()} + ") + inputValue + MakeSelectableForThisUnit($" + {"\"\\\"\"".StringHighlight()}");

                case StringBuilderUnit.AppendMode.Trimmed:
                    return inputValue + MakeSelectableForThisUnit(".Trim()");

                case StringBuilderUnit.AppendMode.Prefixed:
                    string prefix = this.Unit.appendModes[index].prefix;
                    return MakeSelectableForThisUnit($"\"{prefix}\"".StringHighlight()) + MakeSelectableForThisUnit(" + ") + inputValue;

                case StringBuilderUnit.AppendMode.Suffixed:
                    string suffix = this.Unit.appendModes[index].suffix;
                    return inputValue + MakeSelectableForThisUnit(" + ") + MakeSelectableForThisUnit($"\"{suffix}\"".StringHighlight());

                case StringBuilderUnit.AppendMode.Repeated:
                    int repetitions = this.Unit.appendModes[index].repeatCount;
                    List<string> output = new List<string>();
                    for (int i = 0; i < repetitions; i++) output.Add(inputValue);
                    return string.Join(MakeSelectableForThisUnit(" + "), output);
                case StringBuilderUnit.AppendMode.TabAfter:
                    return inputValue + MakeSelectableForThisUnit($" + {"\"\\t\"".StringHighlight()}");
                case StringBuilderUnit.AppendMode.TabBefore:
                    return MakeSelectableForThisUnit($"{"\"\\t\"".StringHighlight()} + ") + inputValue;
                case StringBuilderUnit.AppendMode.CommaSeparated:
                    if (index < totalCount - 1)
                        return inputValue + MakeSelectableForThisUnit($" + {"\", \"".StringHighlight()}");
                    return inputValue;
                case StringBuilderUnit.AppendMode.SpaceAfter:
                    return inputValue + MakeSelectableForThisUnit($" + {"\" \"".StringHighlight()}");
                case StringBuilderUnit.AppendMode.SpaceBefore:
                    return MakeSelectableForThisUnit($"{"\" \"".StringHighlight()} + ") + inputValue;
                case StringBuilderUnit.AppendMode.NewLineBefore:
                    return MakeSelectableForThisUnit($"{"\"\\n\"".StringHighlight()} + ") + inputValue;
                case StringBuilderUnit.AppendMode.NewLineAfter:
                    return inputValue + MakeSelectableForThisUnit($" + {"\"\\n\"".StringHighlight()}");
                case StringBuilderUnit.AppendMode.Delimiter:
                    string delimiter = this.Unit.appendModes[index].delimiter;
                    if (index < totalCount - 1)
                        return inputValue + MakeSelectableForThisUnit($" + {$"\"{delimiter}\"".StringHighlight()}");
                    return inputValue;

                case StringBuilderUnit.AppendMode.Default:
                default:
                    return inputValue;
            }
        }
    }
}