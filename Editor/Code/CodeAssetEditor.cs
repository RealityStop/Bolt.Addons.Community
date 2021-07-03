using UnityEngine;
using UnityEditor;
using Bolt.Addons.Libraries.Humility;
using Bolt.Addons.Libraries.CSharp;
using System;
using Bolt.Addons.Community.Utility.Editor;

namespace Bolt.Addons.Community.Code.Editor
{
    public abstract class CodeAssetEditor<TAsset, TAssetGenerator> : UnityEditor.Editor
        where TAsset : CodeAsset
        where TAssetGenerator : CodeGenerator
    {
        protected TAsset Target;

        protected TAssetGenerator generator;

        private Type lastType;

        protected bool hidden;

        private bool shouldUpdate = true;

        public override bool UseDefaultMargins()
        {
            return false;
        }

        protected virtual bool showOptions => true;
        protected virtual bool showTitle => true;
        protected virtual bool showCategory => true;

        protected virtual void Cache() { shouldUpdate = true; }

        protected virtual void AfterCategoryGUI() { }

        protected virtual void OptionsGUI() { EditorGUILayout.HelpBox("No Options for this asset type", MessageType.None); }

        protected virtual void BeforePreview() { }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            hidden = false;

            Cache();

            if (Target == null) Target = (TAsset)target;

            if (!hidden)
            {
                HUMEditor.Vertical(() =>
                {
                    HUMEditor.Changed(() =>
                    {
                        HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 2, 2), () =>
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

                            AfterCategoryGUI();
                        });

                        EditorGUILayout.Space(8);

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
            }

            if (CSharpPreviewWindow.instance != null)
            {
                if (shouldUpdate) CSharpPreviewWindow.instance.preview.code = CodeGenerator.GetSingleDecorator<TAssetGenerator>(Target);
            }

            shouldUpdate = false;

            EditorUtility.SetDirty(Target);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
