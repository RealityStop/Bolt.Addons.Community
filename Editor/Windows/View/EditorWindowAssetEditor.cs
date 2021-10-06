using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Editor(typeof(EditorWindowAsset))]
    public sealed class EditorWindowAssetEditor : MacroEditor
    {
        public EditorWindowAssetEditor(Metadata metadata) : base(metadata)
        {
        }

        protected override void OnGUI(Rect position, GUIContent label)
        {
            base.OnGUI(position, label);

            GUILayout.Space(4);

            LudiqGUI.InspectorLayout(metadata["variables"]);
        }
    }
}