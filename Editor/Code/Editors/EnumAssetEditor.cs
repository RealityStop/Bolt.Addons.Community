using UnityEngine;
using Unity.VisualScripting;
using UnityEditor;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
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
            Target.itemsOpen = HUMEditor.Foldout(Target.itemsOpen, CommunityStyles.foldoutHeaderColor, Color.black, 1, () => { GUILayout.Label("Items"); }, () =>
            {
                HUMEditor.Vertical().Box(CommunityStyles.foldoutBackgroundColor, Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(1, 1, 0, 1), () =>
                {
                    LudiqGUI.InspectorLayout(items, GUIContent.none);
                });
            });
        }
    }
}
