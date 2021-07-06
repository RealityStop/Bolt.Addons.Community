using UnityEngine;
using UnityEditor;
using Bolt.Addons.Libraries.CSharp;
using System;

namespace Bolt.Addons.Community.Code.Editor
{
    [CustomEditor(typeof(ClassAsset))]
    public sealed class ClassAssetEditor : MemberTypeAssetEditor<ClassAsset, ClassAssetGenerator, ClassFieldDeclaration>
    {
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
        }
    }
}
