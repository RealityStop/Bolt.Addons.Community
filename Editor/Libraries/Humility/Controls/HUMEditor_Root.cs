using System;
using System.Linq;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMEditor
    {
        public static Rect MaximizedWindowSize => new Rect(0, 72, Screen.currentResolution.width, Screen.currentResolution.height - 132 - 42);

        public static Rect ScreenCenter(Vector2 size)
        {
            return new Rect((Screen.currentResolution.width / 2) - (size.x / 2), (Screen.currentResolution.height / 2) - (size.y / 2), size.x, size.y);
        }

        public static Vector2 ScreenCenter(float width, float height)
        {
            return new Vector2((Screen.currentResolution.width / 2) - (width / 2), (Screen.currentResolution.height / 2) - (height / 2));
        }
    }
}
