using System.Collections;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [Inspector(typeof(StringAppendMode))]
    [RenamedFrom("Unity.VisualScripting.Community.AppendType")]
    public class StringAppendModeInspector : Inspector
    {
        public StringAppendModeInspector(Metadata metadata) : base(metadata) { }

        protected override float GetHeight(float width, GUIContent label)
        {
            var target = metadata.value as StringAppendMode;

            float height = EditorGUIUtility.singleLineHeight;

            switch (target.appendMode)
            {
                case StringBuilderUnit.AppendMode.Delimiter:
                    height += EditorGUIUtility.singleLineHeight; // Delimiter input.
                    break;
                case StringBuilderUnit.AppendMode.Prefixed:
                case StringBuilderUnit.AppendMode.Suffixed:
                    height += EditorGUIUtility.singleLineHeight; // Prefix/Suffix input.
                    break;
                case StringBuilderUnit.AppendMode.Repeated:
                    height += EditorGUIUtility.singleLineHeight; // Repeat count input.
                    break;
            }

            return height;
        }

        private string GetModeTooltip(StringBuilderUnit.AppendMode mode)
        {
            if (metadata.Ancestor(m => m.value is StringBuilderUnit) == null) return "";
            return mode switch
            {
                StringBuilderUnit.AppendMode.Default => "Uses the string as is",
                StringBuilderUnit.AppendMode.CommaSeparated => "Adds ',' if this is not the last item",
                StringBuilderUnit.AppendMode.SpaceAfter => "Adds a space after the string",
                StringBuilderUnit.AppendMode.SpaceBefore => "Adds a space before the string",
                StringBuilderUnit.AppendMode.NewLineBefore => "Adds a newline '\\n' before the string",
                StringBuilderUnit.AppendMode.NewLineAfter => "Adds a newline '\\n' after the string",
                StringBuilderUnit.AppendMode.Delimiter => "Adds a custom delimiter between appended strings",
                StringBuilderUnit.AppendMode.UpperCase => "Converts the string to uppercase",
                StringBuilderUnit.AppendMode.LowerCase => "Converts the string to lowercase",
                StringBuilderUnit.AppendMode.Quoted => "Wraps the string in quotation marks",
                StringBuilderUnit.AppendMode.Trimmed => "Removes leading and trailing whitespace from the string",
                StringBuilderUnit.AppendMode.Prefixed => "Adds a custom prefix before the string",
                StringBuilderUnit.AppendMode.Suffixed => "Adds a custom suffix after the string",
                StringBuilderUnit.AppendMode.Repeated => "Repeats the string a specified number of times",
                StringBuilderUnit.AppendMode.TabAfter => "Adds a tab '\\t' after the string",
                StringBuilderUnit.AppendMode.TabBefore => "Adds a tab '\\t' before the string",
                _ => "Unknown mode"
            };
        }

        protected override void OnGUI(Rect position, GUIContent label)
        {
            var target = metadata.value as StringAppendMode;
            LudiqGUI.Inspector(metadata["appendMode"], position, new GUIContent("Mode", GetModeTooltip(target.appendMode)));

            position.y += EditorGUIUtility.singleLineHeight;

            switch (target.appendMode)
            {
                case StringBuilderUnit.AppendMode.Delimiter:
                    LudiqGUI.Inspector(metadata["delimiter"], position, new GUIContent("Delimiter", GetModeTooltip(target.appendMode)));
                    break;
                case StringBuilderUnit.AppendMode.Prefixed:
                    LudiqGUI.Inspector(metadata["prefix"], position, new GUIContent("Prefix", GetModeTooltip(target.appendMode)));
                    break;
                case StringBuilderUnit.AppendMode.Suffixed:
                    LudiqGUI.Inspector(metadata["suffix"], position, new GUIContent("Suffix", GetModeTooltip(target.appendMode)));
                    break;
                case StringBuilderUnit.AppendMode.Repeated:
                    LudiqGUI.Inspector(metadata["repeatCount"], position, new GUIContent("Repeat Count", GetModeTooltip(target.appendMode)));
                    break;
            }
        }
    }
}
