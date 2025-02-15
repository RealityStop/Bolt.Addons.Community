using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMEditor_Flexible_Children
    {
        public static void Box(this HUMEditor.Data.Horizontal horizontal, Color backgroundColor, Color borderColor, int border = 1, Action action = null, bool stretchHorizontal = false, bool stretchVertical = false, params GUILayoutOption[] options)
        {
            var style = new GUIStyle(GUI.skin.box);
            style.normal.background = HUMColor.CacheTexture(backgroundColor);
            style.stretchWidth = stretchHorizontal;
            style.stretchHeight = stretchVertical;
            EditorGUILayout.BeginHorizontal(style, options);
            action?.Invoke();
            EditorGUILayout.EndHorizontal();
        }

        public static void Box(this HUMEditor.Data.Vertical vertical, Color backgroundColor, Color borderColor, int border = 1, Action action = null, bool stretchHorizontal = false, bool stretchVertical = false, params GUILayoutOption[] options)
        {
            var style = new GUIStyle(GUI.skin.box);
            style.normal.background = HUMColor.CacheTexture(backgroundColor);
            style.stretchWidth = stretchHorizontal;
            style.stretchHeight = stretchVertical;
            style.margin = new RectOffset();
            GUILayout.BeginVertical(style, options);
            action?.Invoke();
            GUILayout.EndVertical();
        }

        public static Vector2 ScrollView(this HUMEditor.Data.Flexible flexible, Vector2 scrollPosition, GUIStyle backgroundStyle, Action contents, params GUILayoutOption[] options)
        {
            var output = EditorGUILayout.BeginScrollView(scrollPosition, backgroundStyle, options);
            contents();
            EditorGUILayout.EndScrollView();
            return output;
        }

        public static Vector2 ScrollView(this HUMEditor.Data.Flexible flexible, Vector2 scrollPosition, Action contents, params GUILayoutOption[] options)
        {
            var output = EditorGUILayout.BeginScrollView(scrollPosition, options);
            contents();
            EditorGUILayout.EndScrollView();
            return output;
        }

        public static void Area(this HUMEditor.Data.Flexible flexible, Rect position, Action action)
        {
            UnityEngine.GUILayout.BeginArea(position);
            action();
            UnityEngine.GUILayout.EndArea();
        }

        public static void Space(this HUMEditor.Data.Flexible flexible, float amount)
        {
            EditorGUILayout.Space(amount);
        }

        public static void Box(this HUMEditor.Data.Vertical vertical, Color backgroundColor, Color borderColor, RectOffset padding, TextAnchor contentAlignment, int border = 1, Action contents = null, params GUILayoutOption[] options)
        {
            var style = new GUIStyle();
            var borderStyle = new GUIStyle();
            style.normal.background = HUMColor.CacheTexture(backgroundColor);
            borderStyle.normal.background = HUMColor.CacheTexture(borderColor);
            borderStyle.padding = new RectOffset(border, border, border, border);
            style.padding = padding;
            style.alignment = contentAlignment;

            HUMEditor.Vertical(borderStyle, () =>
            {
                HUMEditor.Vertical(style, () =>
                {
                    contents?.Invoke();
                });
            }, options);
        }

        public static void Box(this HUMEditor.Data.Horizontal horizontal, Color backgroundColor, Color borderColor, RectOffset padding, TextAnchor contentAlignment, int border = 1, Action contents = null, params GUILayoutOption[] options)
        {
            var style = new GUIStyle();
            var borderStyle = new GUIStyle();
            style.normal.background = HUMColor.CacheTexture(backgroundColor);
            borderStyle.normal.background = HUMColor.CacheTexture(borderColor);
            borderStyle.padding = new RectOffset(border, border, border, border);
            style.padding = padding;
            style.alignment = contentAlignment;

            HUMEditor.Horizontal(borderStyle, () =>
            {
                HUMEditor.Horizontal(style, () =>
                {
                    contents?.Invoke();
                });
            }, options);
        }

        public static void Box(this HUMEditor.Data.Vertical vertical, Color backgroundColor, Color borderColor, RectOffset padding, TextAnchor contentAlignment, RectOffset border, Action contents = null, params GUILayoutOption[] options)
        {
            var style = new GUIStyle();
            var borderStyle = new GUIStyle();
            style.normal.background = HUMColor.CacheTexture(backgroundColor);
            borderStyle.normal.background = HUMColor.CacheTexture(borderColor);
            borderStyle.padding = border;
            style.padding = padding;
            style.alignment = contentAlignment;

            HUMEditor.Vertical(borderStyle, () =>
            {
                HUMEditor.Vertical(style, () =>
                {
                    contents?.Invoke();
                });
            }, options);
        }

        public static void Box(this HUMEditor.Data.Horizontal horizontal, Color backgroundColor, Color borderColor, RectOffset padding, TextAnchor contentAlignment, RectOffset border, Action contents = null, params GUILayoutOption[] options)
        {
            var style = new GUIStyle();
            var borderStyle = new GUIStyle();
            style.normal.background = HUMColor.CacheTexture(backgroundColor);
            borderStyle.normal.background = HUMColor.CacheTexture(borderColor);
            borderStyle.padding = border;
            style.padding = padding;
            style.alignment = contentAlignment;

            HUMEditor.Horizontal(borderStyle, () =>
            {
                HUMEditor.Horizontal(style, () =>
                {
                    contents?.Invoke();
                });
            }, options);
        }

        public static void Box(this HUMEditor.Data.Vertical vertical, Color backgroundColor, Color borderColor, RectOffset padding, RectOffset border, Action contents = null, bool stretchVertical = false, bool stretchHorizontal = false, params GUILayoutOption[] options)
        {
            var style = new GUIStyle();
            var borderStyle = new GUIStyle();
            style.normal.background = HUMColor.CacheTexture(backgroundColor);
            borderStyle.normal.background = HUMColor.CacheTexture(borderColor);
            borderStyle.padding = border;
            borderStyle.stretchHeight = stretchVertical;
            borderStyle.stretchWidth = stretchHorizontal;
            style.padding = padding;
            style.stretchHeight = stretchVertical;
            style.stretchWidth = stretchHorizontal;

            HUMEditor.Vertical(borderStyle, () =>
            {
                HUMEditor.Vertical(style, () =>
                {
                    contents?.Invoke();
                });
            }, options);
        }

        public static void Box(this HUMEditor.Data.Horizontal horizontal, Color backgroundColor, Color borderColor, RectOffset padding, RectOffset border, Action contents = null, params GUILayoutOption[] options)
        {
            var style = new GUIStyle();
            var borderStyle = new GUIStyle();
            style.normal.background = HUMColor.CacheTexture(backgroundColor);
            borderStyle.normal.background = HUMColor.CacheTexture(borderColor);
            borderStyle.padding = border;
            style.padding = padding;

            HUMEditor.Horizontal(borderStyle, () =>
            {
                HUMEditor.Horizontal(style, () =>
                {
                    contents?.Invoke();
                });
            }, options);
        }

        public static T EnumFlagsField<T>(
            this HUMEditor.Data.Horizontal horizontal,
            T contents, // No ref, working with a copy
            Color backgroundColor,
            Color borderColor,
            RectOffset padding,
            RectOffset border,
            string separator = ", ",
            GUIContent label = null,
            params GUILayoutOption[] options) where T : Enum
        {
            var style = new GUIStyle { padding = padding };
            var borderStyle = new GUIStyle { padding = border };

            style.normal.background = HUMColor.CacheTexture(backgroundColor);
            borderStyle.normal.background = HUMColor.CacheTexture(borderColor);

            HUMEditor.Horizontal(borderStyle, () =>
            {
                HUMEditor.Horizontal(style, () =>
                {
                    if (!Enum.GetNames(typeof(T)).Contains("None"))
                        throw new InvalidOperationException($"Enum of type {typeof(T)} requires a 'None' value.");

                    if (label != null)
                        GUILayout.Label(label);

                    if (GUILayout.Button(GetSelected(contents, separator), EditorStyles.popup, options))
                    {
                        GenericMenu menu = new GenericMenu();

                        menu.AddItem(new GUIContent("None"), contents.Equals(default(T)), () =>
                        {
                            contents = default;
                            GUI.changed = true; // Force GUI update
                        });

                        foreach (T value in Enum.GetValues(typeof(T)))
                        {
                            if (value.Equals(default(T))) continue;

                            bool isSelected = contents.HasFlag(value);

                            menu.AddItem(new GUIContent(value.ToString()), isSelected, () =>
                            {
                                int intValue = (int)(object)contents; // Get int representation

                                if (isSelected)
                                    intValue &= ~(int)(object)value; // Remove flag
                                else
                                    intValue |= (int)(object)value;  // Add flag

                                contents = (T)(object)intValue; // Cast back to enum
                                GUI.changed = true; // Force GUI update
                            });
                        }

                        menu.ShowAsContext();
                    }
                });
            }, options);

            return contents; // Return modified value
        }

        private static string GetSelected<T>(T contents, string separator) where T : Enum
        {
            if (EqualityComparer<T>.Default.Equals(contents, default))
                return "None";

            return string.Join(separator, Enum.GetValues(typeof(T))
                .Cast<T>()
                .Where(value => !value.Equals(default(T)) && contents.HasFlag(value))
                .Select(value => value.ToString()));
        }
    }
}
