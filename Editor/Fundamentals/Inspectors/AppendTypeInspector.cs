using System.Collections;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [Inspector(typeof(AppendType))]
    public class AppendTypeInspector : Inspector
    {
        public AppendTypeInspector(Metadata metadata) : base(metadata) { }

        protected override float GetHeight(float width, GUIContent label)
        {
            var target = metadata.value as AppendType;

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
        protected override void OnGUI(Rect position, GUIContent label)
        {
            var target = metadata.value as AppendType;

            LudiqGUI.Inspector(metadata["appendMode"], position, new GUIContent("Mode"));

            position.y += EditorGUIUtility.singleLineHeight;

            switch (target.appendMode)
            {
                case StringBuilderUnit.AppendMode.Delimiter:
                    LudiqGUI.Inspector(metadata["delimiter"], position, new GUIContent("Delimiter"));
                    break;
                case StringBuilderUnit.AppendMode.Prefixed:
                    LudiqGUI.Inspector(metadata["prefix"], position, new GUIContent("Prefix"));
                    break;
                case StringBuilderUnit.AppendMode.Suffixed:
                    LudiqGUI.Inspector(metadata["suffix"], position, new GUIContent("Suffix"));
                    break;
                case StringBuilderUnit.AppendMode.Repeated:
                    LudiqGUI.Inspector(metadata["repeatCount"], position, new GUIContent("Repeat Count"));
                    break;
            }
        }
    }
}
