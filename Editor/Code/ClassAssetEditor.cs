using UnityEngine;
using UnityEditor;

namespace Unity.VisualScripting.Community
{
    [CustomEditor(typeof(ClassAsset))]
    public sealed class ClassAssetEditor : MemberTypeAssetEditor<ClassAsset, ClassAssetGenerator, ClassFieldDeclaration, ClassMethodDeclaration, ClassConstructorDeclaration>
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
