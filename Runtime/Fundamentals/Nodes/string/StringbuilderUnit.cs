using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;
using System.Text;

namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// A unit for building and constructing strings.
    /// </summary>
    [UnitTitle("String Builder")]
    [UnitCategory("Community\\Utility\\string")]
    [TypeIcon(typeof(string))]
    [UnitFooterPorts(ValueOutputs = false, ValueInputs = false)]
    public class StringBuilderUnit : Unit
    {
        public enum AppendMode
        {
            Default,           // Append without a separator.
            SpaceAfter,        // Append with a space after.
            SpaceBefore,       // Append with a space before.
            NewLineBefore,     // Append with a newline before.
            NewLineAfter,      // Append with a newline after.
            Delimiter,         // Append with a custom delimiter.
            UpperCase,         // Convert input to uppercase before appending.
            LowerCase,         // Convert input to lowercase before appending.
            Quoted,            // Wrap the input string in quotation marks.
            Trimmed,           // Trim leading and trailing whitespace before appending.
            Prefixed,          // Add a prefix to the string before appending.
            Suffixed,          // Add a suffix to the string after appending.
            Repeated,          // Append the string a given number of times.
            TabAfter,          // Append a tab character (`\t`) after the string.
            TabBefore,         // Append a tab character (`\t`) before the string.
            CommaSeparated,    // Append a comma if not the last input.
        }

        [Inspectable, InspectorLabel("Append Modes", "List to store the modes for each input (Max: 10 items)")]
        [InspectorWide]
        public List<AppendType> appendModes = new List<AppendType>();

        public List<ValueInput> inputPorts { get; private set; } = new List<ValueInput>();
        
        [DoNotSerialize]
        public ValueOutput result;

        private StringBuilder stringBuilder = new StringBuilder();

        private const int MaxInputs = 10;
        private const int MinInputs = 1;

        protected override void Definition()
        {
            inputPorts = new List<ValueInput>();
            AdjustAppendModesList();

            for (int i = 0; i < appendModes.Count; i++)
            {
                AddInputPort(i);
            }

            result = ValueOutput("Result", ConcatenateStrings);
            foreach (var input in inputPorts)
            {
                Requirement(input, result);
            }
        }

        private void AdjustAppendModesList()
        {
            if (appendModes.Count < MinInputs)
            {
                appendModes.Add(new AppendType());
            }

            if (appendModes.Count > MaxInputs)
            {
                appendModes.RemoveAt(appendModes.Count - 1);
            }
        }

        private void AddInputPort(int index)
        {
            var inputPort = ValueInput($"String {index + 1}", "");
            inputPorts.Add(inputPort);
        }

        private string ConcatenateStrings(Flow flow)
        {
            stringBuilder.Clear();

            for (int i = 0; i < appendModes.Count; i++)
            {
                string value = flow.GetValue<string>(inputPorts[i]);
                AppendMode mode = appendModes[i].appendMode;

                switch (mode)
                {
                    case AppendMode.SpaceBefore:
                        stringBuilder.Append(" ");
                        stringBuilder.Append(value);
                        break;

                    case AppendMode.SpaceAfter:
                        stringBuilder.Append(value).Append(" ");
                        break;

                    case AppendMode.NewLineBefore:
                        stringBuilder.AppendLine(value);
                        break;

                    case AppendMode.NewLineAfter:
                        stringBuilder.Append(value).AppendLine();
                        break;

                    case AppendMode.Delimiter:
                        stringBuilder.Append(value);
                        if (i < appendModes.Count - 1) 
                            stringBuilder.Append(appendModes[i].delimiter);
                        break;

                    case AppendMode.UpperCase:
                        stringBuilder.Append(value.ToUpper());
                        break;

                    case AppendMode.LowerCase:
                        stringBuilder.Append(value.ToLower());
                        break;

                    case AppendMode.Quoted:
                        stringBuilder.Append($"\"{value}\"");
                        break;

                    case AppendMode.Trimmed:
                        stringBuilder.Append(value.Trim());
                        break;

                    case AppendMode.Prefixed:
                        stringBuilder.Append(appendModes[i].prefix).Append(value);
                        break;

                    case AppendMode.Suffixed:
                        stringBuilder.Append(value).Append(appendModes[i].suffix);
                        break;

                    case AppendMode.Repeated:
                        for (int j = 0; j < appendModes[i].repeatCount; j++)
                            stringBuilder.Append(value);
                        break;

                    case AppendMode.TabAfter:
                        stringBuilder.Append(value).Append('\t');
                        break;

                    case AppendMode.TabBefore:
                        stringBuilder.Append('\t').Append(value);
                        break;

                    case AppendMode.CommaSeparated:
                        stringBuilder.Append(value);
                        if (i < appendModes.Count - 1) 
                            stringBuilder.Append(", ");
                        break;

                    default:
                        stringBuilder.Append(value);
                        break;
                }
            }

            return stringBuilder.ToString();
        }
    }

    public class AppendType
    {
        public StringBuilderUnit.AppendMode appendMode;
        public string delimiter;      // Used for 'Delimiter' mode.
        public string prefix;         // Used for 'Prefixed' mode.
        public string suffix;         // Used for 'Suffixed' mode.
        public int repeatCount = 1;   // Used for 'Repeated' mode (default to 1).
    }
}