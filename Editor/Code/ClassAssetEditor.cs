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
            Target.scriptableObject = EditorGUILayout.ToggleLeft("Scriptable Object", Target.scriptableObject);
            if (Target.scriptableObject)
            {
                GUILayout.Label("Scriptable Object Menu Name");
                Target.menuName = EditorGUILayout.TextField(Target.menuName);
                GUILayout.Label("Scriptable Object File Name");
                Target.fileName = EditorGUILayout.TextField(Target.fileName);
                GUILayout.Label("Scriptable Object Menu Order");
                Target.order = EditorGUILayout.IntField(Target.order);
            }

            if (Target.scriptableObject)
                return;


            Target.inheritsType = GUILayout.Toggle(Target.inheritsType, "Inherits Type");

            if (Target.inheritsType)
            {
                GUIContent InheritButtonContent = new GUIContent(
                    Target.inherits?.type?.As().CSharpName(false).RemoveHighlights().RemoveMarkdown() ?? "Select Type",
                    Target.inherits?.type?.Icon()?[IconSize.Small]
                );

                lastRect = GUILayoutUtility.GetLastRect();
                if (GUILayout.Button(InheritButtonContent, EditorStyles.popup, GUILayout.MaxHeight(19f)))
                {
                    TypeBuilderWindow.ShowWindow(lastRect, inheritsTypeMeta["type"], false, inheritTypes);
                }
            }
        }
    }
}