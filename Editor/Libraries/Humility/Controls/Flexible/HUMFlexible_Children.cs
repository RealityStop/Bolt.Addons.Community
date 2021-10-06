using System;
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
            EditorGUILayout.BeginVertical(style, options);
            action?.Invoke();
            EditorGUILayout.EndVertical();
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
    }
}
