using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [CustomEditor(typeof(ScriptMachine))]
    public class ScriptMachineCustomEditor : Editor
    {
        private Metadata metadata;

        void OnEnable()
        {
            metadata = Metadata.Root().StaticObject(target);
            if (background == null) background = ColorPalette.unityBackgroundMid.GetPixel();
        }

        private static Texture2D background;

        public override void OnInspectorGUI()
        {
            GraphGUIPatch.InitializeNewGUI();
            var old = LudiqStyles.headerBackground.normal.background;

            LudiqStyles.headerBackground.normal.background = background;
            using (LudiqEditorUtility.editedObject.Override(target))
            {
                var editor = metadata.Editor();
                editor.Draw(GUILayoutUtility.GetRect(
                        EditorGUIUtility.currentViewWidth,
                        LudiqGUI.GetEditorHeight(null, metadata, EditorGUIUtility.currentViewWidth)));
            }
            LudiqStyles.headerBackground.normal.background = old;

            serializedObject.ApplyModifiedProperties();
        }
    }
}