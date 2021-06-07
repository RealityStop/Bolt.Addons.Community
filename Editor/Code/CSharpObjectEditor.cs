using UnityEngine;
using Unity.VisualScripting;
using UnityEditor;
using Bolt.Addons.Libraries.Humility;
using Bolt.Addons.Libraries.CSharp;
using System;

namespace Bolt.Addons.Community.Code.Editor
{
    [CustomEditor(typeof(CSharpObject))]
    public class CSharpObjectEditor : CodeAssetEditor<CSharpObject, CSharpObjectGenerator>
    {
        private Metadata fields;
        private Metadata objectType;
        private SerializedProperty fieldsProp;
        private SerializedProperty objectTypeProp;

        protected override void Cache()
        {
            if (objectType == null)
            {
                objectType = Metadata.FromProperty(serializedObject.FindProperty("objectType"));
                objectTypeProp = serializedObject.FindProperty("objectType");
                hidden = true;
            }

            if (fields == null)
            {
                fields = Metadata.FromProperty(serializedObject.FindProperty("fields"));
                fieldsProp = serializedObject.FindProperty("fields");
                hidden = true;
            }
        }

        protected override void AfterCategoryGUI()
        {
            HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground, Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(1, 1, 1, 1), () =>
            {
                LudiqGUI.InspectorLayout(objectType, GUIContent.none);
            });
        }

        protected override void OptionsGUI()
        {
            Target.serialized = EditorGUILayout.ToggleLeft("Serialized", Target.serialized);
            Target.inspectable = EditorGUILayout.ToggleLeft("Inspectable", Target.inspectable);
            Target.includeInSettings = EditorGUILayout.ToggleLeft("Include In Settings", Target.includeInSettings);
            Target.definedEvent = EditorGUILayout.ToggleLeft("Flag for Defined Event Filtering", Target.definedEvent);
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
