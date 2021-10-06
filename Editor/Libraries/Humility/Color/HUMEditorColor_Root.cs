using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEditor;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMEditorColor
    {
        private static Color _defaultEditorBackground;
        public static Color DefaultEditorBackground
        {
            get
            {
                if (_defaultEditorBackground.a == 0)
                {
                    _defaultEditorBackground = (Color)GetDefaultColorMethod().Invoke(null, null);
                }

                return _defaultEditorBackground;
            }
        }

        private static MethodInfo GetDefaultColorMethod()
        {
            var method = typeof(EditorGUIUtility);
            return method.GetMethod("GetDefaultBackgroundColor", BindingFlags.NonPublic | BindingFlags.Static);
        }
    }
}
