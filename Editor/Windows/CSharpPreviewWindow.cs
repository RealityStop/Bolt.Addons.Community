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

        [SerializeField]
        private CodeAsset asset;

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

            if (asset == null || Selection.activeObject != null && Selection.activeObject != asset)
            {
                asset = Selection.activeObject as CodeAsset;
                if (asset != null) preview.code = CodeGenerator.GetSingleDecorator(asset);
                preview.Refresh();
            }
            else
            {
                if (asset != null)
                {
                    preview.code = CodeGenerator.GetSingleDecorator(asset);
                    if (!cached) { preview.Refresh(); cached = true; }
                }
            }

            preview.DrawLayout();
        }
    }
}