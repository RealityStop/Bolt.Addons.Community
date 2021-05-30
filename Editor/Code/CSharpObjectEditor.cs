using UnityEngine;
using Unity.VisualScripting;
using UnityEditor;
using Bolt.Addons.Integrations.Continuum.Humility;
using Bolt.Addons.Integrations.Continuum.CSharp;
using System;

namespace Bolt.Addons.Community.Code.Editor
{
    [CustomEditor(typeof(CSharpObject))]
    public class CSharpObjectEditor : UnityEditor.Editor
    {
        private Metadata fields;
        private Metadata objectType;
        private SerializedProperty fieldsProp;
        private SerializedProperty objectTypeProp;

        private CSharpObject Target;

        private ICodeGenerator generator;

        public override bool UseDefaultMargins()
        {
            return false;
        }

        public override void OnInspectorGUI()
        {
            var isNull = false;

            if (objectType == null)
            {
                objectType = Metadata.FromProperty(serializedObject.FindProperty("objectType"));
                objectTypeProp = serializedObject.FindProperty("objectType");
                isNull = true;
            }

            if (fields == null)
            {
                fields = Metadata.FromProperty(serializedObject.FindProperty("fields"));
                fieldsProp = serializedObject.FindProperty("fields");
                isNull = true;
            }

            if (Target == null) Target = (CSharpObject)target;

            if (!isNull)
            {
                HUMEditor.Vertical(() =>
                {
                    HUMEditor.Changed(() =>
                    {
                        HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4,4,4,4), new RectOffset(2,2,2,2), () =>
                        {
                            HUMEditor.Horizontal().Box(HUMEditorColor.DefaultEditorBackground, Color.black, new RectOffset(0,0,0,0), new RectOffset(1,1,1,1), () =>
                            {
                                EditorGUILayout.LabelField("Title", GUILayout.Width(80));
                                Target.title = EditorGUILayout.TextField(Target.title);
                            });

                            HUMEditor.Horizontal().Box(HUMEditorColor.DefaultEditorBackground, Color.black, new RectOffset(0, 0, 0, 0), new RectOffset(1, 1, 1, 1), () =>
                            {
                                EditorGUILayout.LabelField("Category", GUILayout.Width(80));
                                Target.category = EditorGUILayout.TextField(Target.category);
                            });

                            EditorGUILayout.Space(4);

                            HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground, Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(1, 1, 1, 1), () =>
                            {
                                LudiqGUI.InspectorLayout(objectType, GUIContent.none);
                            });
                        });

                        EditorGUILayout.Space(8);

                        Target.optionsOpened = HUMEditor.Foldout(Target.optionsOpened, HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, 2, () => { GUILayout.Label("Options"); }, () =>
                        {
                            HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 0, 2), () =>
                            {
                                Target.serialized = EditorGUILayout.ToggleLeft("Serialized", Target.serialized);
                                Target.inspectable = EditorGUILayout.ToggleLeft("Inspectable", Target.inspectable);
                                Target.includeInSettings = EditorGUILayout.ToggleLeft("Include In Settings", Target.includeInSettings);
                                Target.definedEvent = EditorGUILayout.ToggleLeft("Flag for Defined Event Filtering", Target.definedEvent);
                            });
                        });

                        EditorGUILayout.Space(8);

                        Target.fieldsOpened = HUMEditor.Foldout(Target.fieldsOpened, HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, 2, () => { GUILayout.Label("Fields"); }, () =>
                        {
                             HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 0, 2), () => 
                             {
                                 LudiqGUI.InspectorLayout(fields, GUIContent.none);
                             });
                        });

                        EditorGUILayout.Space(8);

                    }, () => 
                    {
                        generator = CSharpObjectGenerator.GetSingleDecorator(Target);
                    });
                    
                    Target.preview = HUMEditor.Foldout(Target.preview, HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, 2, ()=> { GUILayout.Label("Preview C#"); }, () =>
                    {
                        HUMEditor.Vertical().Box(HUMColor.Grey(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 0, 2), () =>
                        {
                            generator = CSharpObjectGenerator.GetSingleDecorator<CSharpObjectGenerator>(Target);
                            GUILayout.Label(generator.Generate(0).RemoveMarkdown(), new GUIStyle(GUI.skin.label) { richText = true });
                        }, true, true);
                    });
                });
            }
        }
    }
}
