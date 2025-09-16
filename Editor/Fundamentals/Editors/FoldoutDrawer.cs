using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace Unity.VisualScripting.Community
{
    [CustomPropertyDrawer(typeof(FoldoutAttribute))]
    public class FoldoutDrawer : PropertyDrawer
    {
        private static Dictionary<string, bool> foldoutStates = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var foldout = (FoldoutAttribute)attribute;

            if (!foldoutStates.ContainsKey(foldout.group))
                foldoutStates[foldout.group] = foldout.startExpanded;

            bool expanded = foldoutStates[foldout.group];

            if (IsFirstPropertyInGroup(property, out var groupProperties))
            {
                Rect foldoutRect = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                foldoutStates[foldout.group] = EditorGUI.Foldout(foldoutRect, expanded, foldout.group, true);
                position.y += EditorGUIUtility.singleLineHeight;
            }

            if (expanded)
            {
                Rect labelRect = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(labelRect, property.displayName);

                float fieldHeight = EditorGUI.GetPropertyHeight(property, true);
                Rect fieldRect = new(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, fieldHeight);
                EditorGUI.PropertyField(fieldRect, property, GUIContent.none, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var foldout = (FoldoutAttribute)attribute;

            if (!foldoutStates.TryGetValue(foldout.group, out var expanded))
                expanded = foldout.startExpanded;

            float height = 0;

            if (IsFirstPropertyInGroup(property, out _))
            {
                height += EditorGUIUtility.singleLineHeight;
            }

            if (expanded)
            {
                height += EditorGUIUtility.singleLineHeight;
                height += EditorGUI.GetPropertyHeight(property, true) + 2;
            }

            return height;
        }

        private bool IsFirstPropertyInGroup(SerializedProperty property, out List<FieldInfo> groupFields)
        {
            groupFields = new List<FieldInfo>();

            var fields = property.serializedObject.targetObject
                .GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var field in fields)
            {
                var attrs = (FoldoutAttribute[])field.GetCustomAttributes(typeof(FoldoutAttribute), false);
                if (attrs.Length > 0 && attrs[0].group == ((FoldoutAttribute)attribute).group)
                {
                    groupFields.Add(field);
                }
            }

            if (groupFields.Count > 0)
                return groupFields[0].Name == property.name;

            return false;
        }
    }
}