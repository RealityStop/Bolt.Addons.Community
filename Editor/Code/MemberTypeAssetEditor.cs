using UnityEngine;
using Unity.VisualScripting;
using UnityEditor;
using Bolt.Addons.Libraries.Humility;

namespace Bolt.Addons.Community.Code.Editor
{
    public abstract class MemberTypeAssetEditor<TMemberTypeAsset, TMemberTypeGenerator, TFieldDeclaration> : CodeAssetEditor<TMemberTypeAsset, TMemberTypeGenerator>
        where TMemberTypeAsset : MemberTypeAsset<TFieldDeclaration>
        where TMemberTypeGenerator : MemberTypeAssetGenerator<TMemberTypeAsset, TFieldDeclaration>
        where TFieldDeclaration : FieldDeclaration
    {
        protected Metadata fields;
        protected SerializedProperty fieldsProp;

        protected override void Cache()
        {
            base.Cache();

            if (fields == null)
            {
                fields = Metadata.FromProperty(serializedObject.FindProperty("fields"));
                fieldsProp = serializedObject.FindProperty("fields");
                hidden = true;
            }
        }

        protected override void AfterCategoryGUI()
        {
            //HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground, Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(1, 1, 1, 1), () =>
            //{
            //});
        }

        protected override void OptionsGUI()
        {
            Target.serialized = EditorGUILayout.ToggleLeft("Serialized", Target.serialized);
            Target.inspectable = EditorGUILayout.ToggleLeft("Inspectable", Target.inspectable);
            Target.includeInSettings = EditorGUILayout.ToggleLeft("Include In Settings", Target.includeInSettings);
            Target.definedEvent = EditorGUILayout.ToggleLeft("Flag for Defined Event Filtering", Target.definedEvent);
            OnExtendedOptionsGUI();
        }

        protected virtual void OnExtendedOptionsGUI()
        {

        }

        protected override void BeforePreview()
        {
            Target.fieldsOpened = HUMEditor.Foldout(Target.fieldsOpened, HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, 2, () => { GUILayout.Label("Fields"); }, () =>
            {
                HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 0, 2), () =>
                {
                    LudiqGUI.InspectorLayout(fields, GUIContent.none);
                });
            });
        }
    }
}
