using UnityEngine;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using System;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    public sealed class CSharpPreview
    {
        string output = string.Empty;

        [SerializeField]
        private Vector2 scrollPosition;

        public static Color background => HUMColor.Grey(0.1f);

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
            scrollPosition = HUMEditor.Draw().ScrollView(scrollPosition, () =>
            {
                HUMEditor.Vertical().Box(background, Color.black, action: () =>
                {
                    if (code != null)
                    {
                        if (shouldRefresh)
                        {
                            output = code.Generate(0);
                            shouldRefresh = false;
                            output = output.Replace("/*", "<color=#CC3333>/*");
                            output = output.Replace("*/", "*/</color>");
                            output = output.RemoveMarkdown();
                            labelStyle = new GUIStyle(GUI.skin.label) { richText = true, stretchWidth = true, stretchHeight = true, alignment = TextAnchor.UpperLeft, wordWrap = true };
                            labelStyle.normal.background = null;
                            shouldRepaint = true;
                        }
                    }

                    if (labelStyle == null)
                    {
                        labelStyle = new GUIStyle(GUI.skin.label) { richText = true, stretchWidth = true, stretchHeight = true, alignment = TextAnchor.UpperLeft, wordWrap = true };
                        labelStyle.normal.background = null;
                    }

                    GUILayout.Label(output, labelStyle);

                }, stretchHorizontal: true, stretchVertical: true);
            });
        }
    }
}