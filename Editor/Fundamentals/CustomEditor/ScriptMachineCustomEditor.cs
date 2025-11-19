using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [CustomEditor(typeof(ScriptMachine))]
    public class ScriptMachineCustomEditor : Editor
    {
        private Metadata metadata;

        private Metadata nestMetadata => metadata[nameof(IMachine.nest)];

        private Metadata graphMetadata => nestMetadata[nameof(IGraphNest.graph)];

        protected Metadata headerTitleMetadata => graphMetadata[nameof(IGraph.title)];

        protected Metadata headerSummaryMetadata => graphMetadata[nameof(IGraph.summary)];

        protected bool showHeader => graphMetadata.value != null;

        void OnEnable()
        {
            metadata = Metadata.Root().StaticObject(target);
            if (background == null) background = ColorPalette.unityBackgroundMid.GetPixel();
        }

        private static Texture2D background;

        public override void OnInspectorGUI()
        {
            var old = LudiqStyles.headerBackground.normal.background;

            LudiqStyles.headerBackground.normal.background = background;

            if (showHeader)
            {
                Rect headerRect = GUILayoutUtility.GetRect(
                    EditorGUIUtility.currentViewWidth,
                    LudiqGUI.GetHeaderHeight(headerTitleMetadata.Inspector(), headerTitleMetadata, headerSummaryMetadata, null, EditorGUIUtility.currentViewWidth)
                );
                OnHeaderGUI(headerRect);
            }

            OnNestGUI();

            LudiqStyles.headerBackground.normal.background = old;

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void OnHeaderGUI(Rect headerPosition)
        {
            var y = 0f;
            LudiqGUI.OnHeaderGUI(headerTitleMetadata, headerSummaryMetadata, null, headerPosition, ref y);
        }

        protected virtual void OnNestGUI()
        {
            LudiqGUI.EditorLayout(nestMetadata);
        }
    }
}
