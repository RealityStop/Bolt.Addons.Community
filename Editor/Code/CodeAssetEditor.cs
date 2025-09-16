using UnityEngine;
using UnityEditor;
using Unity.VisualScripting.Community.Libraries.Humility;
using System;

namespace Unity.VisualScripting.Community.CSharp
{
    public abstract class CodeAssetEditor<TAsset, TAssetGenerator> : UnityEditor.Editor
        where TAsset : CodeAsset
        where TAssetGenerator : CodeGenerator
    {
        protected TAsset Target;

        protected TAssetGenerator generator;

        private Type lastType;
        private bool _shouldUpdate = true;
        protected bool shouldUpdate
        {
            get
            {
                return _shouldUpdate;
            }

            set
            {
                Target.shouldRefresh = value;
                _shouldUpdate = value;
            }
        }

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
            Undo.undoRedoPerformed += UpdatePreview;
            UpdatePreview();
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= UpdatePreview;
        }

        protected virtual void AfterCategoryGUI() { }

        protected virtual void OptionsGUI() { EditorGUILayout.HelpBox("No Options for this asset type", MessageType.None); }

        protected virtual void BeforePreview() { }

        protected virtual void OnTypeHeaderGUI()
        {
            if (showTitle)
            {
                GraphWindow.active?.context?.BeginEdit();
                HUMEditor.Horizontal().Box(HUMEditorColor.DefaultEditorBackground, Color.black, new RectOffset(0, 0, 0, 0), new RectOffset(1, 1, 1, 1), () =>
                {
                    EditorGUILayout.LabelField("Title", GUILayout.Width(80));

                    EditorGUI.BeginChangeCheck();
                    string newTitle = EditorGUILayout.TextField(Target.title);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RegisterCompleteObjectUndo(Target, "Change Asset Title");
                        Target.title = newTitle;
                        EditorUtility.SetDirty(Target);
                        UpdatePreview();
                    }
                });
                GraphWindow.active?.context?.EndEdit();
            }

            if (showCategory)
            {
                HUMEditor.Horizontal().Box(HUMEditorColor.DefaultEditorBackground, Color.black, new RectOffset(0, 0, 0, 0), new RectOffset(1, 1, 1, 1), () =>
                {
                    EditorGUILayout.LabelField("Category", GUILayout.Width(80));

                    EditorGUI.BeginChangeCheck();
                    string newCategory = EditorGUILayout.TextField(Target.category);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RegisterCompleteObjectUndo(Target, "Change Asset Category");
                        Target.category = newCategory;
                        EditorUtility.SetDirty(Target);
                        UpdatePreview();
                    }
                });
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            // CSharpPreviewWindow.instance?.preview.Refresh();
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
                    UpdatePreview();
                });

            });

            serializedObject.ApplyModifiedProperties();
        }

        protected void UpdatePreview()
        {
            CSharpPreviewWindow.RefreshPreview();
        }
    }
}