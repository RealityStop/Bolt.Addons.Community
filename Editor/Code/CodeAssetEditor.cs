using UnityEngine;
using UnityEditor;
using Unity.VisualScripting.Community.Libraries.Humility;
using System;

namespace Unity.VisualScripting.Community
{
    public abstract class CodeAssetEditor<TAsset, TAssetGenerator> : UnityEditor.Editor
        where TAsset : CodeAsset
        where TAssetGenerator : CodeGenerator
    {
        protected TAsset Target;

        protected TAssetGenerator generator;

        private Type lastType;

        protected bool shouldUpdate = true;

        public override bool UseDefaultMargins()
        {
            return false;
        }

        protected virtual bool showOptions => true;
        protected virtual bool showTitle => true;
        protected virtual bool showCategory => true;

        protected bool cached;

        private bool warningPresent;

        protected virtual void OnEnable()
        {
            if (Target == null) Target = (TAsset)target;

            if (!EditorPrefs.HasKey("Bolt.Addons.Community.Code.Warning_Present"))
            {
                EditorPrefs.SetBool("Bolt.Addons.Community.Code.Warning_Present", true);
                warningPresent = true;
            }
            else
            {
                warningPresent = EditorPrefs.GetBool("Bolt.Addons.Community.Code.Warning_Present");
            }

            shouldUpdate = true;
        }

        protected virtual void AfterCategoryGUI() { }

        protected virtual void OptionsGUI() { EditorGUILayout.HelpBox("No Options for this asset type", MessageType.None); }

        protected virtual void BeforePreview() { }

        protected virtual void OnTypeHeaderGUI()
        {
            if (showTitle)
            {
                HUMEditor.Horizontal().Box(HUMEditorColor.DefaultEditorBackground, Color.black, new RectOffset(0, 0, 0, 0), new RectOffset(1, 1, 1, 1), () =>
                {
                    EditorGUILayout.LabelField("Title", GUILayout.Width(80));
                    Target.title = EditorGUILayout.TextField(Target.title);
                });
            }


            if (showCategory)
            {
                HUMEditor.Horizontal().Box(HUMEditorColor.DefaultEditorBackground, Color.black, new RectOffset(0, 0, 0, 0), new RectOffset(1, 1, 1, 1), () =>
                {
                    EditorGUILayout.LabelField("Category", GUILayout.Width(80));
                    Target.category = EditorGUILayout.TextField(Target.category);
                });
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            CSharpPreviewWindow.instance?.preview.Refresh();
            HUMEditor.Vertical(() =>
            {
                HUMEditor.Changed(() =>
                {
                    HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 2, 2), () =>
                    {
                        OnTypeHeaderGUI();
                        AfterCategoryGUI();
                    });

                    EditorGUILayout.Space(4);

                    if (warningPresent)
                    {
                        EditorGUILayout.HelpBox("Code Assets are an in preview feature. " +
                            "Not all functionality is present, and not all nodes have working generators. " +
                            "There is no guarentee assets will remain in tact. " +
                            "You may move the output scipts somewhere else for safe keeping.", MessageType.Warning);

                        EditorGUILayout.Space(4);

                        if (GUILayout.Button("Understood. Hide this warning."))
                        {
                            EditorPrefs.SetBool("Bolt.Addons.Community.Code.Warning_Present", false);
                            warningPresent = false;
                        }
                    }

                    EditorGUILayout.Space(4);

                    if (showOptions)
                    {
                        Target.optionsOpened = HUMEditor.Foldout(Target.optionsOpened, HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, 2, () => { GUILayout.Label("Options"); }, () =>
                        {
                            HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 0, 2), () =>
                            {
                                OptionsGUI();
                            });
                        });

                        EditorGUILayout.Space(4);
                    }

                    BeforePreview();

                    EditorGUILayout.Space(4);

                }, () =>
                {
                    shouldUpdate = true;
                });

            });

            if (CSharpPreviewWindow.instance != null)
            {
                if (shouldUpdate)
                {
                    CSharpPreviewWindow.instance.preview.code = CodeGenerator.GetSingleDecorator<TAssetGenerator>(Target);
                    CSharpPreviewWindow.instance.preview.Refresh();
                }
            }

            shouldUpdate = false;

            EditorUtility.SetDirty(Target);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
