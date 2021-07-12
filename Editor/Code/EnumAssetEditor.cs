using UnityEngine;
using Unity.VisualScripting;
using UnityEditor;
using Bolt.Addons.Libraries.Humility;

namespace Bolt.Addons.Community.Code.Editor
{
    [CustomEditor(typeof(EnumAsset))]
    public class EnumAssetEditor : CodeAssetEditor<EnumAsset, EnumAssetGenerator>
    {
        private Metadata items;
        private SerializedProperty itemsProp;

        protected override void Cache()
        {
            base.Cache();

            if (items == null)
            {
                items = Metadata.FromProperty(serializedObject.FindProperty("items"));
                itemsProp = serializedObject.FindProperty("items");
                cached = true;
            }
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
