using UnityEngine;
using UnityEditor;
using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityObject = UnityEngine.Object;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Unity.VisualScripting.Community
{
    [CustomEditor(typeof(ClassAsset))]
    public sealed class ClassAssetEditor : MemberTypeAssetEditor<ClassAsset, ClassAssetGenerator, ClassFieldDeclaration, ClassMethodDeclaration, ClassConstructorDeclaration>
    {
        private Metadata inheritsTypeMeta;
        private Type[] inheritTypes;
        private Rect lastRect;

        protected override void OnEnable()
        {
            base.OnEnable();
            inheritsTypeMeta ??= Metadata.FromProperty(serializedObject.FindProperty("inherits"));
            inheritTypes = GetAllInheritableTypes();
        }

        private Type[] GetAllInheritableTypes()
        {
            List<Type> inheritableTypes = new List<Type>();

            var types = Codebase.settingsAssembliesTypes.Where(t =>
                t.Is().Inheritable() &&
                t != typeof(string) &&
                t != typeof(GameObject) &&
                t != typeof(Unity.VisualScripting.Community.Libraries.CSharp.Void) &&
                t != typeof(void) &&
                !!TypeHasSpecialName(t)
            ).ToArray();

            inheritableTypes.AddRange(types);

            inheritableTypes.Add(typeof(MonoBehaviour));

            var editorTypes = Codebase.editorTypes.Where(t =>
                t.Is().Inheritable() &&
                !TypeHasSpecialName(t)
            ).ToArray();

            inheritableTypes.AddRange(editorTypes);

            return inheritableTypes.ToArray();
        }

        private bool TypeHasSpecialName(Type t)
        {
            return t.IsSpecialName || t.IsDefined(typeof(CompilerGeneratedAttribute));
        }

        protected override void OnExtendedOptionsGUI()
        {
            EditorGUI.BeginChangeCheck();
            var scriptableObject = EditorGUILayout.ToggleLeft("Scriptable Object", Target.scriptableObject);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(Target, "Toggled Asset Option 'ScriptableObject'");
                Target.scriptableObject = scriptableObject;
                EditorUtility.SetDirty(Target);
                shouldUpdate = true;
            }
            if (Target.scriptableObject)
            {
                GUILayout.Label("Scriptable Object Menu Name");
                EditorGUI.BeginChangeCheck();
                var menuName = EditorGUILayout.TextField(Target.menuName);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RegisterCompleteObjectUndo(Target, "Edited Asset Option 'ScriptableObject' value 'Menu Name'");
                    Target.menuName = menuName;
                    EditorUtility.SetDirty(Target);
                    shouldUpdate = true;
                }
                GUILayout.Label("Scriptable Object File Name");
                EditorGUI.BeginChangeCheck();
                var fileName = EditorGUILayout.TextField(Target.fileName);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RegisterCompleteObjectUndo(Target, "Edited Asset Option 'ScriptableObject' value 'File Name'");
                    Target.fileName = fileName;
                    EditorUtility.SetDirty(Target);
                    shouldUpdate = true;
                }
                GUILayout.Label("Scriptable Object Menu Order");
                EditorGUI.BeginChangeCheck();
                var order = EditorGUILayout.IntField(Target.order);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RegisterCompleteObjectUndo(Target, "Edited Asset Option 'ScriptableObject' value 'Order'");
                    Target.order = order;
                    EditorUtility.SetDirty(Target);
                    shouldUpdate = true;
                }
            }
            if (Target.scriptableObject)
                return;

            EditorGUI.BeginChangeCheck();
            var inheritsType = GUILayout.Toggle(Target.inheritsType, "Inherits Type");
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(Target, "Toggled Asset Option 'inheritsType'");
                Target.inheritsType = inheritsType;
                EditorUtility.SetDirty(Target);
                shouldUpdate = true;
            }

            if (Target.inheritsType)
            {
                GUIContent InheritButtonContent = new GUIContent(
                    Target.inherits?.type?.As().CSharpName(false).RemoveHighlights().RemoveMarkdown() ?? "Select Type",
                    Target.inherits?.type?.Icon()?[IconSize.Small]
                );

                lastRect = GUILayoutUtility.GetLastRect();
                if (GUILayout.Button(InheritButtonContent, EditorStyles.popup, GUILayout.MaxHeight(19f)))
                {
                    TypeBuilderWindow.ShowWindow(lastRect, inheritsTypeMeta["type"], false, inheritTypes, () =>
                    {
                        Undo.RegisterCompleteObjectUndo(Target, "Changed Asset Inherited Type");
                        if (CSharpPreviewWindow.instance != null)
                        {
                            if (CSharpPreviewWindow.instance.showCodeWindow)
                            {
                                CSharpPreviewWindow.instance.UpdateCodeDisplay();
                            }
                        }
                    });
                }
            }
        }
    }
}