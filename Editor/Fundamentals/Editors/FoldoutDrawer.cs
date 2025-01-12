using System;
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
                if (property.GetUnderlyingField().HasAttribute<FoldoutEndAttribute>())
                {
                    return;
                }
                while (property.Next(false) && !property.GetUnderlyingField().HasAttribute<FoldoutEndAttribute>())
                {
                    position.y += EditorGUIUtility.singleLineHeight + space;
                    EditorGUI.PropertyField(position, property, new GUIContent(property.name), true);
                }
                try
                {
                    if (property.GetUnderlyingField().HasAttribute<FoldoutEndAttribute>())
                    {
                        position.y += EditorGUIUtility.singleLineHeight + space;
                        EditorGUI.PropertyField(position, property, new GUIContent(property.name), true);
                    }
                }
                catch (MissingMemberException)
                {
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float expandedHight = EditorGUI.GetPropertyHeight(property, label, true) + (EditorGUIUtility.singleLineHeight + space);
            if (property.GetUnderlyingField().HasAttribute<FoldoutEndAttribute>())
            {
                return isExpanded ? expandedHight : EditorGUIUtility.singleLineHeight;
            }
            while (property.Next(false) && !property.GetUnderlyingField().HasAttribute<FoldoutEndAttribute>())
            {
                expandedHight += EditorGUI.GetPropertyHeight(property, new GUIContent(property.name), true) + space;
            }
            try
            {
                if (property.GetUnderlyingField().HasAttribute<FoldoutEndAttribute>())
                {
                    expandedHight += EditorGUI.GetPropertyHeight(property, new GUIContent(property.name), true) + space;
                }
            }
            catch (MissingMemberException)
            {
            }
            return isExpanded ? expandedHight : EditorGUIUtility.singleLineHeight;
        }
    }
}