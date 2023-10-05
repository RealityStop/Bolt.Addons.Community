using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using UnityEditor;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    public sealed class CSharpPreview
    {
        string output = string.Empty;

        [SerializeField]
        private Vector2 scrollPosition;

        #region Colors

        [SerializeField]
        public static Color VariableColor;

        [SerializeField]
        public static Color TypeColor;

        [SerializeField]
        public static Color ConstructColor;

        [SerializeField]
        public static Color EnumColor;

        [SerializeField]
        public static Color InterfaceColor;

        [SerializeField]
        public static Color StringColor;

        [SerializeField]
        public static Color NumericColor;

        #endregion

        public static bool ShowSubgraphComment = true;

        public static bool AutomaticallyGetTransform = true;

        //public static bool UseCustomEventsAsMethods = true;

        public static Color background => HUMColor.Grey(0.1f);
        public static Color Settingsbackground => HUMColor.Grey(0.2f);

        private GUIStyle labelStyle;

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
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (ShowCodeWindow)
            {
                GUILayout.Label("Zoom", GUILayout.Width(40));
                float newZoomFactor = EditorGUILayout.Slider(zoomFactor, 0.5f, 2.0f, GUILayout.Width(150));
                if (!Mathf.Approximately(newZoomFactor, zoomFactor))
                {
                    zoomFactor = newZoomFactor;
                    shouldRefresh = true;
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Copy to Clipboard", EditorStyles.toolbarButton, GUILayout.Width(150)))
                {
                    string outputWithoutColorTags = Regex.Replace(output, @"<[^>]+>|&[^;]+;", "");
                    EditorGUIUtility.systemCopyBuffer = outputWithoutColorTags;
                }

                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(80)))
                {
                    shouldRefresh = true;
                }
            }

            if (!ShowCodeWindow)
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Preview", EditorStyles.toolbarButton))
                {
                    ShowCodeWindow = true;
                }
            }
            else
            {
                if (GUILayout.Button("Settings", EditorStyles.toolbarButton))
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


        private void CodeWindow()
        {
            CodeBuilder.VariableColor = VariableColor.ToHexString();

            if (shouldRefresh)
            {
                if (code != null)
                {
                    output = code.Generate(0);
                }
                else
                {
                    output = "";
                }

                output = output.Replace("/*", "<color=#CC3333>/*");
                output = output.Replace("*/", "*/</color>");
                output = output.RemoveMarkdown();

                labelStyle = new GUIStyle(GUI.skin.label)
                {
                    richText = true,
                    stretchWidth = true,
                    stretchHeight = true,
                    alignment = TextAnchor.UpperLeft,
                    wordWrap = true,
                    fontSize = (int)(17.0f * zoomFactor)
                };
                labelStyle.normal.background = null;
                shouldRefresh = false;
                shouldRepaint = true;
            }

            scrollPosition = HUMEditor.Draw().ScrollView(scrollPosition, () =>
            {
                HUMEditor.Vertical().Box(background, Color.black, action: () =>
                {
                    GUILayout.Label(output, labelStyle);
                }, stretchHorizontal: true, stretchVertical: true);
            });
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

                    GUILayout.Label("Code Settings", sectionTitleStyle);
                    GUILayout.Space(10);

                    Rect labelRect = GUILayoutUtility.GetLastRect();

                    EditorGUILayout.BeginHorizontal();
                    DrawLabelWithTooltip("Show Subgraph Comment:", "Shows a comment where the Subgraph and Port are being generated.", labelRect, GUILayout.Width(200));
                    ShowSubgraphComment = EditorGUILayout.Toggle(ShowSubgraphComment);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    DrawLabelWithTooltip("Automatically Get Transform:", "Generates code to get the transform if a transform node is used. No need for 'GameObject Get Transform'.", labelRect, GUILayout.Width(200));
                    AutomaticallyGetTransform = EditorGUILayout.Toggle(AutomaticallyGetTransform);
                    EditorGUILayout.EndHorizontal();

                    /*EditorGUILayout.BeginHorizontal();
                    DrawLabelWithTooltip("Custom Events As Methods:", "Generate the code for Trigger Custom Event as a Method Trigger and a Custom Event as a Method.", labelRect, GUILayout.Width(200));
                    UseCustomEventsAsMethods = EditorGUILayout.Toggle(UseCustomEventsAsMethods);
                    EditorGUILayout.EndHorizontal();*/

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
            Settings.AutomaticallyGetTransform = AutomaticallyGetTransform;
        }
    }
}
