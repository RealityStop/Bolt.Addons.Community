using Bolt.Addons.Community.Code.Editor;
using System;
using UnityEditor;
using UnityEngine;

namespace Bolt.Addons.Community.Utility.Editor
{
    [Serializable]
    public sealed class CSharpPreviewWindow : EditorWindow
    {
        public static CSharpPreviewWindow instance;

        [SerializeReference]
        public CSharpPreview preview = new CSharpPreview();

        private bool cached = false;

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
            if (!cached)
            {
                preview.code = CodeGenerator.GetSingleDecorator((Selection.activeObject as CodeAsset));
                preview.shouldRepaint = true;
                preview.Refresh();
                cached = true;
            }
            preview.DrawLayout();
            if (preview.shouldRepaint)
            {
                preview.Refresh();
                Repaint();
            }
        }
    }
}