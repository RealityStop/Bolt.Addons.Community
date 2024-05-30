using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Linq;
using UnityEditor;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    public sealed class CSharpPreview
    {
        public string output = string.Empty;

        [SerializeField]
        private Vector2 scrollPosition;

        #region Colors

        [SerializeField]
        public static Color VariableColor;

        [SerializeField]
        public static Color StringColor;

        [SerializeField]
        public static Color NumericColor;

        [SerializeField]
        public static Color ConstructColor;

        [SerializeField]
        public static Color TypeColor;

        [SerializeField]
        public static Color EnumColor;

        [SerializeField]
        public static Color InterfaceColor;

        #endregion

        public static bool isSelectable = true;

        public static bool ShowSubgraphComment = true;

        public static Color background => HUMColor.Grey(0.1f);
        public static Color Settingsbackground => HUMColor.Grey(0.2f);

        private GUIStyle labelStyle;
        private GUIStyle readOnlyTextStyle;
        private GUIStyle lineNumberStyle;

        [SerializeField]
        private bool shouldRefresh = true;
        public bool shouldRepaint = true;

        [SerializeReference]
        private ICodeGenerator _code;
        public ICodeGenerator code
        {
            get => _code;
            set
            {
                _code = value;
                RefreshIfNeeded();
            }
        }

        [SerializeField]
        private float zoomFactor = 0.7f;
        [SerializeField]
        private bool ShowCodeWindow = true;

        public void RefreshIfNeeded()
        {
            if (shouldRefresh)
            {
                Refresh();
            }
        }

        public void Refresh()
        {
            shouldRefresh = true;
            CSharpPreviewWindow.instance?.Repaint();
        }

        public void DrawLayout()
        {
            var path = "Assets/Unity.VisualScripting.Community.Generated/";
            HUMIO.Ensure(path).Path();
            CSharpPreviewSettings Settings = AssetDatabase.LoadAssetAtPath<CSharpPreviewSettings>(path + "CSharpPreviewSettings.asset");
            CodeBuilder.VariableColor = Settings.VariableColor.ToHexString();
            CodeBuilder.TypeColor = Settings.TypeColor.ToHexString();
            CodeBuilder.ConstructColor = Settings.ConstructColor.ToHexString();
            CodeBuilder.EnumColor = Settings.EnumColor.ToHexString();
            CodeBuilder.InterfaceColor = Settings.InterfaceColor.ToHexString();
            CodeBuilder.StringColor = Settings.StringColor.ToHexString();
            CodeBuilder.NumericColor = Settings.NumericColor.ToHexString();
            if (ShowCodeWindow)
                HandleZoomEvents();
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUI.enabled = ShowCodeWindow;

            GUILayout.Label("Zoom", LudiqStyles.toolbarLabel);
            float newZoomFactor = GUILayout.HorizontalSlider(zoomFactor, 0.5f, 2.0f, GUILayout.Width(150));
            GUILayout.Label(newZoomFactor.ToString("0.#") + "x", LudiqStyles.toolbarLabel);

            if (!Mathf.Approximately(newZoomFactor, zoomFactor))
            {
                zoomFactor = newZoomFactor;
                shouldRefresh = true;
            }

            GUILayout.FlexibleSpace();

            EditorGUI.BeginDisabledGroup(code == null);

            if (GUILayout.Button("Compile", LudiqStyles.toolbarButton, GUILayout.Width(80)))
            {
                UnityEditor.Selection.activeObject = CSharpPreviewWindow.asset;
                AssetCompiler.CompileSelected();
            }

            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Copy to Clipboard", LudiqStyles.toolbarButton, GUILayout.Width(150)))
            {
                var outputToCopy = RemoveColorTags(output);
                outputToCopy = CodeUtility.RemoveCustomHighlights(outputToCopy);
                outputToCopy = CodeUtility.RemoveAllSelectableTags(outputToCopy);
                EditorGUIUtility.systemCopyBuffer = outputToCopy;
            }

            if (GUILayout.Button("Utility Window", LudiqStyles.toolbarButton, GUILayout.Width(110)))
            {
                UtilityWindow.Open();
            }

            if (GUILayout.Button("Refresh", LudiqStyles.toolbarButton, GUILayout.Width(80)))
            {
                shouldRefresh = true;
            }

            GUI.enabled = true;

            if (!ShowCodeWindow)
            {
                if (GUILayout.Button("Preview", LudiqStyles.toolbarButton, GUILayout.Width(80)))
                {
                    ShowCodeWindow = true;
                }
            }
            else
            {
                if (GUILayout.Button("Settings", LudiqStyles.toolbarButton, GUILayout.Width(80)))
                {
                    ShowCodeWindow = false;
                }
            }

            EditorGUILayout.EndHorizontal();

            if (ShowCodeWindow)
            {
                CodeWindow();
            }
            else
            {
                SettingsWindow();
            }
        }

        private void HandleZoomEvents()
        {
            Event e = Event.current;

            if (e.type == EventType.ScrollWheel && e.control)
            {
                float zoomChange = e.delta.y * -0.05f;
                zoomFactor = Mathf.Clamp(zoomFactor + zoomChange, 0.5f, 2.0f);
                shouldRefresh = true;
                e.Use();
            }
        }

        private string RemoveColorTags(string input)
        {
            return Regex.Replace(input, @"<color=#[0-9a-fA-F]{6,8}>|</color>", string.Empty);
        }

        private void CodeWindow()
        {
            if (shouldRefresh)
            {
                if (code != null)
                {
                    output = code.Generate(0);
                }

                if (output.Length > 0)
                {
                    output = Regex.Replace(output, @"/\*(?!.*\(Recommendation\))", "<color=#CC3333>/*");
                    output = output.Replace("*/", "*/</color>");
                    output = output.RemoveMarkdown();
                }

                readOnlyTextStyle = new GUIStyle(GUI.skin.textArea)
                {
                    richText = true,
                    stretchWidth = true,
                    wordWrap = false,
                    fontSize = (int)(EditorGUIUtility.singleLineHeight * zoomFactor),
                    normal = { textColor = Color.white, background = null },
                    border = new RectOffset(0, 0, 0, 0),
                    padding = new RectOffset(10, 10, 10, 10)
                };

                lineNumberStyle = new GUIStyle(GUI.skin.label)
                {
                    richText = true,
                    alignment = TextAnchor.UpperRight,
                    fontSize = (int)(EditorGUIUtility.singleLineHeight * zoomFactor),
                    normal = { textColor = Color.grey, background = null },
                    border = new RectOffset(0, 0, 0, 0),
                    padding = new RectOffset(10, 10, 10, 10)
                };

                shouldRefresh = false;
                shouldRepaint = true;
            }

            string[] lines = output.Split(new[] { '\n' }, StringSplitOptions.None);
            int lineCount = lines.Length;

            var lineNumbers = string.Join("\n", Enumerable.Range(1, lineCount).Select(i => i.ToString()));

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, true, true);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(40));
            GUILayout.Label(lineNumbers, lineNumberStyle);
            GUILayout.EndVertical();

            if (isSelectable)
            {
                if (GraphWindow.activeContext != null)
                {
                    var selection = GraphWindow.active.context.canvas.selection;
                    var code = selection
                        .Where(selected => selected is Unit)
                        .Cast<Unit>()
                        .SelectMany(unit =>
                        {
                            if (unit.controlInputs.Any(input => input.hasValidConnection))
                            {
                                return unit.controlInputs
                                    .Where(input => input.hasValidConnection)
                                    .Select(input => unit.GenerateControl(input, new ControlGenerationData(), 0).RemoveMarkdown());
                            }
                            else
                            {
                                if (unit.controlOutputs.Any(output => output.hasValidConnection))
                                {
                                    return new List<string>
                                    {
                                    unit.GenerateControl(null, new ControlGenerationData(), 0).RemoveMarkdown()
                                    };
                                }
                                else return Enumerable.Empty<string>();
                            }
                        })
                        .ToList();

                    var codeString = string.Join("\n", code);

                    var codeLinesToHighlight = codeString.Split('\n').ToList();

                    foreach (var item in GraphWindow.active.context.canvas.selection)
                    {
                        output = CodeUtility.HighlightCode(output, item.ToString());
                    }

                    GUILayout.TextArea(CodeUtility.RemoveAllSelectableTags(output), readOnlyTextStyle, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                }
                else
                {
                    GUILayout.TextArea(CodeUtility.RemoveAllSelectableTags(output), readOnlyTextStyle, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                }
            }
            else
            {
                GUILayout.Label(output, readOnlyTextStyle);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();
        }

        private void SettingsWindow()
        {
            scrollPosition = HUMEditor.Draw().ScrollView(scrollPosition, () =>
            {
                HUMEditor.Vertical().Box(Settingsbackground, Color.black, action: () =>
                {
                    var windowTitleStyle = new GUIStyle(GUI.skin.label)
                    {
                        richText = true,
                        fontSize = 20
                    };

                    var sectionTitleStyle = new GUIStyle(GUI.skin.label)
                    {
                        richText = true,
                        fontSize = 15
                    };

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("C# Preview Settings", windowTitleStyle);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

                    // GUILayout.Label("Code Generation Settings", sectionTitleStyle);
                    // GUILayout.Space(10);

                    // Rect labelRect = GUILayoutUtility.GetLastRect();

                    // EditorGUILayout.BeginHorizontal();
                    // DrawLabelWithTooltip("Show Subgraph Comments:", "A comment where the Subgraph and Port are being generated.", labelRect, GUILayout.Width(200));
                    // ShowSubgraphComment = EditorGUILayout.Toggle(ShowSubgraphComment);
                    // EditorGUILayout.EndHorizontal();

                    GUILayout.Space(10);

                    GUILayout.Label("Text Colors", sectionTitleStyle);
                    GUILayout.Space(10);
                    DrawColorField("Variable Color", ref VariableColor, "#26CCCC");
                    DrawColorField("String Color", ref StringColor, "#CC8833");
                    DrawColorField("Numeric Color", ref NumericColor, "#DDFFBB");
                    DrawColorField("Construct Color", ref ConstructColor, "#4488FF");
                    DrawColorField("Type Color", ref TypeColor, "#33EEAA");
                    DrawColorField("Enum Color", ref EnumColor, "#FFFFBB");
                    DrawColorField("Interface Color", ref InterfaceColor, "#DDFFBB");

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Save Settings", GUILayout.Width(150), GUILayout.Height(30)))
                    {
                        SaveSettings();
                    }

                }, stretchHorizontal: true, stretchVertical: true);
            });
        }


        private void DrawLabelWithTooltip(string labelText, string tooltipText, Rect labelRect, params GUILayoutOption[] options)
        {
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            Color normalColor = Color.white;
            Color hoverColor = new Color(0.8f, 0.8f, 1.0f);
            labelStyle.normal.textColor = normalColor;
            bool isMouseOverLabel = labelRect.Contains(Event.current.mousePosition);

            if (isMouseOverLabel)
            {
                labelStyle.normal.textColor = hoverColor;
            }
            GUIContent labelContent = new GUIContent(labelText, tooltipText);
            GUILayout.Label(labelContent, labelStyle, options);
        }

        private void DrawColorField(string label, ref Color color, string defaultHex)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(label + ":", GUILayout.Width(200));
            EditorGUILayout.BeginHorizontal();
            color = EditorGUILayout.ColorField(color, GUILayout.ExpandWidth(true));
            CodeBuilder.VariableColor = color.ToHexString();
            if (GUILayout.Button("Default", GUILayout.Width(60)))
            {
                UnityEngine.ColorUtility.TryParseHtmlString(defaultHex, out color);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();
        }

        private void SaveSettings()
        {
            var path = "Assets/Unity.VisualScripting.Community.Generated/";
            HUMIO.Ensure(path).Path();
            CSharpPreviewSettings Settings = AssetDatabase.LoadAssetAtPath<CSharpPreviewSettings>(path + "CSharpPreviewSettings.asset");

            Settings.NumericColor = NumericColor;
            Settings.EnumColor = EnumColor;
            Settings.ConstructColor = ConstructColor;
            Settings.VariableColor = VariableColor;
            Settings.StringColor = StringColor;
            Settings.InterfaceColor = InterfaceColor;
            Settings.TypeColor = TypeColor;
            Settings.ShowSubgraphComment = ShowSubgraphComment;
            EditorPrefs.SetString("VariableColor", VariableColor.ToHexString());
            EditorPrefs.SetString("StringColor", StringColor.ToHexString());
            EditorPrefs.SetString("NumericColor", NumericColor.ToHexString());
            EditorPrefs.SetString("ConstructColor", ConstructColor.ToHexString());
            EditorPrefs.SetString("TypeColor", TypeColor.ToHexString());
            EditorPrefs.SetString("EnumColor", EnumColor.ToHexString());
            EditorPrefs.SetString("InterfaceColor", InterfaceColor.ToHexString());
            EditorPrefs.SetBool("ShowSubgraphComments", ShowSubgraphComment);
            AssetDatabase.SaveAssets();
        }

        public void LoadSettings()
        {
            var path = "Assets/Unity.VisualScripting.Community.Generated/";
            HUMIO.Ensure(path).Path();
            CSharpPreviewSettings Settings = AssetDatabase.LoadAssetAtPath<CSharpPreviewSettings>(path + "CSharpPreviewSettings.asset");
            if (EditorPrefs.HasKey("VariableColor")) UnityEngine.ColorUtility.TryParseHtmlString("#" + EditorPrefs.GetString("VariableColor"), out Settings.VariableColor);
            if (EditorPrefs.HasKey("StringColor")) UnityEngine.ColorUtility.TryParseHtmlString("#" + EditorPrefs.GetString("StringColor"), out Settings.StringColor);
            if (EditorPrefs.HasKey("NumericColor")) UnityEngine.ColorUtility.TryParseHtmlString("#" + EditorPrefs.GetString("NumericColor"), out Settings.NumericColor);
            if (EditorPrefs.HasKey("ConstructColor")) UnityEngine.ColorUtility.TryParseHtmlString("#" + EditorPrefs.GetString("ConstructColor"), out Settings.ConstructColor);
            if (EditorPrefs.HasKey("TypeColor")) UnityEngine.ColorUtility.TryParseHtmlString("#" + EditorPrefs.GetString("TypeColor"), out Settings.TypeColor);
            if (EditorPrefs.HasKey("EnumColor")) UnityEngine.ColorUtility.TryParseHtmlString("#" + EditorPrefs.GetString("EnumColor"), out Settings.EnumColor);
            if (EditorPrefs.HasKey("InterfaceColor")) UnityEngine.ColorUtility.TryParseHtmlString("#" + EditorPrefs.GetString("InterfaceColor"), out Settings.InterfaceColor);
            ShowSubgraphComment = EditorPrefs.GetBool("ShowSubgraphComments", true);
        }
    }
}