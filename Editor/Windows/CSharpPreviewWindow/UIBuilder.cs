using UnityEditor;
using UnityEngine;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    internal static class UIBuilder
    {
        public static void DrawCSharpPreviewSettings(CSharpPreviewSettings settings, System.Action onChanged)
        {
            SectionLabel("C# Preview Settings", 15, 0);

            GUILayout.BeginVertical(GetBoxStyle());
            {
                DrawToggleRow("Show Subgraph Comment :", "Generate a comment where the Subgraph and Port are being generated. Useful for navigating the code.", ref settings.showSubgraphComment, () =>
                {
                    CSharpPreviewSettings.ShouldShowSubgraphComment = settings.showSubgraphComment;
                    settings.SaveAndDirty();
                    onChanged?.Invoke();
                });

                DrawToggleRow("Show Recommendations :", "Show recommendations if there is a better way of generating the code.", ref settings.showRecommendations, () =>
                {
                    CSharpPreviewSettings.ShouldShowRecommendations = settings.showRecommendations;
                    settings.SaveAndDirty();
                    onChanged?.Invoke();
                });

                DrawToggleRow("Show Tooltips :", "Show extra infomation when hovering over most comments with the (Hover for more info) tag.", ref settings.showTooltips, () =>
                {
                    CSharpPreviewSettings.ShouldGenerateTooltips = settings.showTooltips;
                    settings.SaveAndDirty();
                    onChanged?.Invoke();
                });

                DrawIntFieldRow("Recursion Depth :", "Adjust recursion depth for complex graphs. If you see 'Infinite recursion', try increasing this value. Setting too high may hurt performance.", ref settings.recursionDepth, () =>
                {
                    CSharpPreviewSettings.RecursionDepth = settings.recursionDepth;
                    settings.SaveAndDirty();
                    onChanged?.Invoke();
                });
            }
            GUILayout.EndVertical();

            SectionLabel("Syntax Highlights", 15, 0);

            GUILayout.BeginVertical(GetBoxStyle());
            {
                DrawColorRow("Variable Color :", "The variable color.", ref settings.VariableColor, "#00FFFF", val =>
                {
                    CodeBuilder.VariableColor = val.ToHexString();
                    settings.SaveAndDirty();
                    onChanged?.Invoke();
                });
                GUILayout.Space(5);
                DrawColorRow("String Color :", "The color for strings.", ref settings.StringColor, "#CC8833", val =>
                {
                    CodeBuilder.StringColor = val.ToHexString();
                    settings.SaveAndDirty();
                    onChanged?.Invoke();
                });
                GUILayout.Space(5);
                DrawColorRow("Numeric Color :", "The color for numeric values.", ref settings.NumericColor, "#DDFFBB", val =>
                {
                    CodeBuilder.NumericColor = val.ToHexString();
                    settings.SaveAndDirty();
                    onChanged?.Invoke();
                });
                GUILayout.Space(5);
                DrawColorRow("Construct Color :", "The color for constructs (e.g., public, private, int, float).", ref settings.ConstructColor, "#4488FF", val =>
                {
                    CodeBuilder.ConstructColor = val.ToHexString();
                    settings.SaveAndDirty();
                    onChanged?.Invoke();
                });
                GUILayout.Space(5);
                DrawColorRow("Type Color :", "The color for data types.", ref settings.TypeColor, "#33EEAA", val =>
                {
                    CodeBuilder.TypeColor = val.ToHexString();
                    settings.SaveAndDirty();
                    onChanged?.Invoke();
                });
                GUILayout.Space(5);
                DrawColorRow("Enum Color :", "The color for enums.", ref settings.EnumColor, "#FFFFBB", val =>
                {
                    CodeBuilder.EnumColor = val.ToHexString();
                    settings.SaveAndDirty();
                    onChanged?.Invoke();
                });
                GUILayout.Space(5);
                DrawColorRow("Interface Color :", "The color for interfaces.", ref settings.InterfaceColor, "#DDFFBB", val =>
                {
                    CodeBuilder.InterfaceColor = val.ToHexString();
                    settings.SaveAndDirty();
                    onChanged?.Invoke();
                });
            }
            GUILayout.EndVertical();
        }

        #region Helpers

        private static GUIStyle GetBoxStyle()
        {
            return new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(5, 10, 10, 10),
                margin = new RectOffset(5, 5, 5, 5)
            };
        }

        private static void SectionLabel(string text, int fontSize = 14, int topMargin = 10, Color? color = null)
        {
            GUILayout.Space(topMargin);
            var style = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = fontSize,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = color ?? Color.white },
                padding = new RectOffset(4, 4, 2, 2)
            };
            GUILayout.Label(text, style);
        }

        private static void DrawToggleRow(string label, string tooltip, ref bool value, System.Action onChanged, float labelWidth = 200)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent(label, tooltip), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
            var newValue = EditorGUILayout.Toggle(value, GUILayout.Width(20));
            if (newValue != value)
            {
                value = newValue;
                onChanged?.Invoke();
            }
            GUILayout.EndHorizontal();
        }

        private static void DrawIntFieldRow(string label, string tooltip, ref int value, System.Action onChanged, float labelWidth = 200)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent(label, tooltip), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
            var newValue = EditorGUILayout.DelayedIntField(value, GUILayout.Width(100));
            if (newValue != value)
            {
                value = newValue;
                onChanged?.Invoke();
            }
            GUILayout.EndHorizontal();
        }

        private static void DrawColorRow(string label, string tooltip, ref Color value, string defaultHex, System.Action<Color> onChanged, float labelWidth = 200, float fieldWidth = 300)
        {
            GUILayout.BeginHorizontal();
            {
                var oldWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = labelWidth;
                EditorGUILayout.LabelField(new GUIContent(label, tooltip), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                EditorGUIUtility.labelWidth = oldWidth;

                Rect rect = GUILayoutUtility.GetRect(fieldWidth, EditorGUIUtility.singleLineHeight);
                var newValue = EditorGUI.ColorField(new Rect(rect.x + 25, rect.y, fieldWidth - 25 - 70, rect.height), GUIContent.none, value, true, true, false);
                if (newValue != value)
                {
                    value = newValue;
                    onChanged?.Invoke(newValue);
                }

                if (GUI.Button(new Rect(rect.xMax - 70, rect.y, 70, rect.height), "Default"))
                {
                    if (UnityEngine.ColorUtility.TryParseHtmlString(defaultHex, out var defaultValue))
                    {
                        defaultValue.a = 1f;
                        value = defaultValue;
                        onChanged?.Invoke(defaultValue);
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        #endregion
    }
}
