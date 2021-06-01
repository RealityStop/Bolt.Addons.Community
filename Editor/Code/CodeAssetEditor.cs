using UnityEngine;
using UnityEditor;
using Bolt.Addons.Integrations.Continuum.Humility;
using Bolt.Addons.Integrations.Continuum.CSharp;

namespace Bolt.Addons.Community.Code.Editor
{
    public abstract class CodeAssetEditor<TAsset, TAssetGenerator> : UnityEditor.Editor
        where TAsset : CodeAsset
        where TAssetGenerator : CodeGenerator
    {
        protected TAsset Target;

        protected TAssetGenerator generator;

        protected bool hidden;

        public override bool UseDefaultMargins()
        {
            return false;
        }

        protected virtual void Cache() { }

        protected virtual void AfterCategoryGUI() { }

        protected virtual void OptionsGUI() { EditorGUILayout.HelpBox("No Options for this asset type", MessageType.None); }

        protected virtual void BeforePreview() { }

        public override sealed void OnInspectorGUI()
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
                            HUMEditor.Horizontal().Box(HUMEditorColor.DefaultEditorBackground, Color.black, new RectOffset(0, 0, 0, 0), new RectOffset(1, 1, 1, 1), () =>
                            {
                                EditorGUILayout.LabelField("Title", GUILayout.Width(80));
                                Target.title = EditorGUILayout.TextField(Target.title);
                            });

                            HUMEditor.Horizontal().Box(HUMEditorColor.DefaultEditorBackground, Color.black, new RectOffset(0, 0, 0, 0), new RectOffset(1, 1, 1, 1), () =>
                            {
                                EditorGUILayout.LabelField("Category", GUILayout.Width(80));
                                Target.category = EditorGUILayout.TextField(Target.category);
                            });

                            AfterCategoryGUI();
                        });

                        EditorGUILayout.Space(8);

                        Target.optionsOpened = HUMEditor.Foldout(Target.optionsOpened, HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, 2, () => { GUILayout.Label("Options"); }, () =>
                        {
                            HUMEditor.Vertical().Box(HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 0, 2), () =>
                            {
                                OptionsGUI();
                            });
                        });

                        EditorGUILayout.Space(4);

                        BeforePreview();

                        EditorGUILayout.Space(4);

                    }, () =>
                    {
                        generator = CodeGenerator.GetSingleDecorator<TAssetGenerator>(Target);
                    });

                    Target.preview = HUMEditor.Foldout(Target.preview, HUMEditorColor.DefaultEditorBackground.Darken(0.1f), Color.black, 2, () => { GUILayout.Label("Preview C#"); }, () =>
                    {
                        HUMEditor.Vertical().Box(HUMColor.Grey(0.1f), Color.black, new RectOffset(4, 4, 4, 4), new RectOffset(2, 2, 0, 2), () =>
                        {
                            generator = CodeGenerator.GetSingleDecorator<TAssetGenerator>(Target);
                            GUILayout.Label(generator.Generate(0).RemoveMarkdown(), new GUIStyle(GUI.skin.label) { richText = true, wordWrap = true });
                        }, true, true);
                    });
                });
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
