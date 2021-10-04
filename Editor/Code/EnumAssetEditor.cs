using UnityEngine;
using Unity.VisualScripting;
using UnityEditor;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    [CustomEditor(typeof(EnumAsset))]
    public class EnumAssetEditor : CodeAssetEditor<EnumAsset, EnumAssetGenerator>
    {
        private Metadata items;
        private SerializedProperty itemsProp;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (items == null)
            {
                items = Metadata.FromProperty(serializedObject.FindProperty("items"));
                itemsProp = serializedObject.FindProperty("items");
            }

            shouldUpdate = true;
        }

        protected override void OptionsGUI()
        {
            Target.useIndex = EditorGUILayout.ToggleLeft("Use Index", Target.useIndex);
        }

        protected override void BeforePreview()
        {
            Target.itemsOpen = HUMEditor.Foldout(Target.itemsOpen, HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, 2, () => { GUILayout.Label("Items"); }, () =>
            {
                HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 0, 2), () =>
                {
                    LudiqGUI.InspectorLayout(items, GUIContent.none);
                });
            });
        }
    }
}
