using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [CustomPropertyDrawer(typeof(FoldoutAttribute))]
    public class FoldoutDrawer : PropertyDrawer
    {
        private bool isExpanded = false;
        const int space = 2;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            FoldoutAttribute foldout = (FoldoutAttribute)attribute;

            EditorGUI.indentLevel = 0;
            isExpanded = EditorGUI.Foldout(
                new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
                isExpanded, foldout.header, true
            );

            if (isExpanded)
            {
                EditorGUI.indentLevel++;
                position.y += EditorGUIUtility.singleLineHeight + space;
                EditorGUI.PropertyField(position, property, new GUIContent(property.name), true);
                while (property.Next(false) && !property.GetUnderlyingField().HasAttribute<FoldoutEndAttribute>())
                {
                    position.y += EditorGUIUtility.singleLineHeight + space;
                    EditorGUI.PropertyField(position, property, new GUIContent(property.name), true);
                }
                if (property.GetUnderlyingField().HasAttribute<FoldoutEndAttribute>())
                {
                    position.y += EditorGUIUtility.singleLineHeight + space;
                    EditorGUI.PropertyField(position, property, new GUIContent(property.name), true);
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float expandedHight = EditorGUI.GetPropertyHeight(property, label, true) + (EditorGUIUtility.singleLineHeight + space);
            while (property.Next(false) && !property.GetUnderlyingField().HasAttribute<FoldoutEndAttribute>())
            {
                expandedHight += EditorGUI.GetPropertyHeight(property, new GUIContent(property.name), true) + space;
            }
            if (property.GetUnderlyingField().HasAttribute<FoldoutEndAttribute>())
            {
                expandedHight += EditorGUI.GetPropertyHeight(property, new GUIContent(property.name), true) + space;
            }
            return isExpanded ? expandedHight : EditorGUIUtility.singleLineHeight;
        }
    }
}