using System;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    public sealed class CSharpPreviewWindow : EditorWindow
    {
        public static CSharpPreviewWindow instance;
        public static Event e;

        [SerializeReference]
        public CSharpPreview preview = new CSharpPreview();

        private CodeAsset codeAsset;
        private ScriptGraphAsset graphAsset;
        private bool cached;

        [MenuItem("Window/Community Addons/C# Preview")]
        public static void Open()
        {
            CSharpPreviewWindow window = GetWindow<CSharpPreviewWindow>();
            window.titleContent = new GUIContent("C# Preview");
            instance = window;
        }

        private void OnEnable()
        {
            instance = this;
            preview.shouldRepaint = true;
            preview.Refresh();
        }

        private void OnGUI()
        {
            e = Event.current;
            codeAsset = Selection.activeObject as CodeAsset;
            graphAsset = Selection.activeObject as ScriptGraphAsset;

            if (codeAsset != null)
            {
                preview.code = CodeGenerator.GetSingleDecorator(codeAsset);
                cached = true;
            }
            else if (graphAsset != null && GraphWindow.active != null)
            {
                preview.code = CodeGenerator.GetSingleDecorator(graphAsset);
                cached = true;
            }
            else
            {
                cached = false;
            }

            if (!cached)
            {
                preview.Refresh();
                cached = true;
            }

            preview.DrawLayout();
        }
    }
}
