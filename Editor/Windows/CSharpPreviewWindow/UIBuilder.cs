using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEditor;

namespace Unity.VisualScripting.Community
{
    internal static class UIBuilder
    {
        public static void DrawCSharpPreviewSettings(CSharpPreviewSettings settings, System.Action onChanged)
        {
            SectionLabel("C# Preview Settings", 18, 10);

            GUILayout.BeginVertical("box");
            {
                SectionLabel("Generation Settings", 14, 5);

                var showSubgraph = Toggle("Show Subgraph Comment :", "Generate a comment where the Subgraph and Port are being generated. Useful for navigating the code.", settings.showSubgraphComment);

                if (showSubgraph != settings.showSubgraphComment)
                {
                    settings.showSubgraphComment = showSubgraph;
                    CSharpPreviewSettings.ShouldShowSubgraphComment = showSubgraph;
                    settings.SaveAndDirty();
                    onChanged?.Invoke();
                }

                var showRecommendations = Toggle("Show Recommendations :", "Show recommendations if there is a better way of generating the code.", settings.showRecommendations);

                if (showRecommendations != settings.showRecommendations)
                {
                    settings.showRecommendations = showRecommendations;
                    CSharpPreviewSettings.ShouldShowRecommendations = showRecommendations;
                    settings.SaveAndDirty();
                    onChanged?.Invoke();
                }

                var showTooltips = Toggle("Show Tooltips :", "Show tooltips in places where there is a problem.", settings.showTooltips);

                if (showTooltips != settings.showTooltips)
                {
                    settings.showTooltips = showTooltips;
                    CSharpPreviewSettings.ShouldGenerateTooltips = showTooltips;
                    settings.SaveAndDirty();
                    onChanged?.Invoke();
                }

                var recursionDepth = IntField("Recursion Depth :", "Adjust recursion depth for complex graphs. If you see 'Infinite recursion', try increasing this value. Setting too high may hurt performance.", settings.recursionDepth);

                if (recursionDepth != settings.recursionDepth)
                {
                    settings.recursionDepth = recursionDepth;
                    CSharpPreviewSettings.RecursionDepth = recursionDepth;
                    settings.SaveAndDirty();
                    onChanged?.Invoke();
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            {
                SectionLabel("Syntax Highlights", 14, 5);

                ColorFieldWithDefault("Variable Color :", "The variable color.", settings.VariableColor, newValue =>
                    {
                        settings.VariableColor = newValue;
                        CodeBuilder.VariableColor = newValue.ToHexString();
                        settings.SaveAndDirty();
                        onChanged?.Invoke();
                    },
                    () =>
                    {
                        if (UnityEngine.ColorUtility.TryParseHtmlString("#00FFFF", out var value))
                        {
                            value.a = 1f;
                            settings.VariableColor = value;
                            CodeBuilder.VariableColor = "00FFFF";
                            onChanged?.Invoke();
                        }
                    });

                ColorFieldWithDefault("String Color :", "The color for strings.", settings.StringColor, newValue =>
                    {
                        settings.StringColor = newValue;
                        CodeBuilder.StringColor = newValue.ToHexString();
                        settings.SaveAndDirty();
                        onChanged?.Invoke();
                    },
                    () =>
                    {
                        if (UnityEngine.ColorUtility.TryParseHtmlString("#CC8833", out var value))
                        {
                            value.a = 1f;
                            settings.StringColor = value;
                            CodeBuilder.StringColor = "CC8833";
                            onChanged?.Invoke();
                        }
                    });

                ColorFieldWithDefault("Numeric Color :", "The color for numeric values.", settings.NumericColor, newValue =>
                    {
                        settings.NumericColor = newValue;
                        CodeBuilder.NumericColor = newValue.ToHexString();
                        settings.SaveAndDirty();
                        onChanged?.Invoke();
                    },
                    () =>
                    {
                        if (UnityEngine.ColorUtility.TryParseHtmlString("#DDFFBB", out var value))
                        {
                            value.a = 1f;
                            settings.NumericColor = value;
                            CodeBuilder.NumericColor = "DDFFBB";
                            onChanged?.Invoke();
                        }
                    });

                ColorFieldWithDefault("Construct Color :", "The color for constructs (e.g., public, private, int, float).", settings.ConstructColor, newValue =>
                    {
                        settings.ConstructColor = newValue;
                        CodeBuilder.ConstructColor = newValue.ToHexString();
                        settings.SaveAndDirty();
                        onChanged?.Invoke();
                    },
                    () =>
                    {
                        if (UnityEngine.ColorUtility.TryParseHtmlString("#4488FF", out var value))
                        {
                            value.a = 1f;
                            settings.ConstructColor = value;
                            CodeBuilder.ConstructColor = "4488FF";
                            onChanged?.Invoke();
                        }
                    });

                ColorFieldWithDefault("Type Color :", "The color for data types.", settings.TypeColor, newValue =>
                    {
                        settings.TypeColor = newValue;
                        CodeBuilder.TypeColor = newValue.ToHexString();
                        settings.SaveAndDirty();
                        onChanged?.Invoke();
                    },
                    () =>
                    {
                        if (UnityEngine.ColorUtility.TryParseHtmlString("#33EEAA", out var value))
                        {
                            value.a = 1f;
                            settings.TypeColor = value;
                            CodeBuilder.TypeColor = "33EEAA";
                            onChanged?.Invoke();
                        }
                    });

                ColorFieldWithDefault("Enum Color :", "The color for enums.", settings.EnumColor, newValue =>
                    {
                        settings.EnumColor = newValue;
                        CodeBuilder.EnumColor = newValue.ToHexString();
                        settings.SaveAndDirty();
                        onChanged?.Invoke();
                    },
                    () =>
                    {
                        if (UnityEngine.ColorUtility.TryParseHtmlString("#FFFFBB", out var value))
                        {
                            value.a = 1f;
                            settings.EnumColor = value;
                            CodeBuilder.EnumColor = "FFFFBB";
                            onChanged?.Invoke();
                        }
                    });

                ColorFieldWithDefault("Interface Color :", "The color for interfaces.", settings.InterfaceColor, newValue =>
                    {
                        settings.InterfaceColor = newValue;
                        CodeBuilder.InterfaceColor = newValue.ToHexString();
                        settings.SaveAndDirty();
                        onChanged?.Invoke();
                    },
                    () =>
                    {
                        if (UnityEngine.ColorUtility.TryParseHtmlString("#DDFFBB", out var value))
                        {
                            value.a = 1f;
                            settings.InterfaceColor = value;
                            CodeBuilder.InterfaceColor = "DDFFBB";
                            onChanged?.Invoke();
                        }
                    });
            }
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Draws a color field with a label + "Default" button, GUILayout-based.
        /// </summary>
        public static void ColorFieldWithDefault(string label, string tooltip, Color value, System.Action<Color> onChanged, System.Action onReset, float labelWidth = 200, float fieldWidth = 400)
        {
            GUILayout.BeginHorizontal();
            {
                var oldContent = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = labelWidth;

                EditorGUILayout.LabelField(new GUIContent(label, tooltip),
                    EditorStyles.boldLabel,
                    GUILayout.Width(labelWidth));

                EditorGUIUtility.labelWidth = oldContent;

                var newValue = EditorGUILayout.ColorField(value, GUILayout.Width(fieldWidth));
                if (newValue != value)
                    onChanged?.Invoke(newValue);

                if (GUILayout.Button("Default", GUILayout.Width(70)))
                {
                    onReset?.Invoke();
                }
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws a bold section label.
        /// </summary>
        public static void SectionLabel(string text, int fontSize = 14, int topMargin = 10)
        {
            var style = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = fontSize
            };

            GUILayout.Space(topMargin);
            GUILayout.Label(text, style);
        }

        /// <summary>
        /// Draws a toggle with label.
        /// </summary>
        public static bool Toggle(string label, string tooltip, bool value, float labelWidth = 200)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent(label, tooltip), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
            var newValue = EditorGUILayout.Toggle(value, GUILayout.Width(20));
            GUILayout.EndHorizontal();
            return newValue;
        }

        /// <summary>
        /// Draws an int field with label.
        /// </summary>
        public static int IntField(string label, string tooltip, int value, float labelWidth = 200)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent(label, tooltip), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
            var newValue = EditorGUILayout.IntField(value, GUILayout.Width(100));
            GUILayout.EndHorizontal();
            return newValue;
        }
    }
}
