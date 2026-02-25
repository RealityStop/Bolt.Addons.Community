using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMEvents_Children
    {
        /// <summary>
        /// The time since the mouse was last clicked.
        /// </summary>
        public static float lastClickTime = 0f;
        public static Vector2 lastLeftClickPosition;
        public static Vector2 lastRightClickPosition;

        public static HUMEvents.Data.Is Is(this HUMEvents.Data.Mouse mouse)
        {
            return new HUMEvents.Data.Is(mouse);
        }

        public static bool Inside(this HUMEvents.Data.Is @is, Rect position)
        {
            return position.Contains(@is.mouse.e.mousePosition);
        }

        public static bool Released(this HUMEvents.Data.Left left)
        {
            HUMEvents.clickCount--;
            return left.mouse.e.button == 0 && left.mouse.e.type == EventType.MouseUp;
        }

        public static bool Released(this HUMEvents.Data.Middle middle)
        {
            HUMEvents.clickCount--;
            return middle.mouse.e.button == 2 && middle.mouse.e.type == EventType.MouseUp;
        }

        public static bool Released(this HUMEvents.Data.Right right)
        {
            HUMEvents.clickCount--;
            return right.mouse.e.button == 1 && right.mouse.e.type == EventType.MouseUp;
        }

        public static bool Clicked(this HUMEvents.Data.Left left)
        {
            HUMEvents.clickCount++;
            return left.mouse.e.button == 0 && left.mouse.e.type == EventType.MouseDown;
        }

        public static bool Clicked(this HUMEvents.Data.Middle middle)
        {
            HUMEvents.clickCount++;
            return middle.mouse.e.button == 2 && middle.mouse.e.type == EventType.MouseDown;
        }

        public static bool Clicked(this HUMEvents.Data.Right right)
        {
            HUMEvents.clickCount++;
            return right.mouse.e.button == 1 && right.mouse.e.type == EventType.MouseDown;
        }

        /// <summary>
        /// Returns true if the mouse was double left clicked.
        /// </summary>
        public static bool Clicked(this HUMEvents.Data.DoubleLeft left, float elapseTime)
        {
            if ((lastClickTime - Time.time) * -1 < elapseTime)
            {
                lastClickTime = Time.time;
                return true;
            }

            lastClickTime = Time.time;
            return false;
        }

        /// <summary>
        /// Begins an action that happens twice in a row with the mouse.
        /// </summary>
        public static HUMEvents.Data.Double Double(this HUMEvents.Data.Mouse mouse)
        {
            return new HUMEvents.Data.Double(mouse);
        }

        /// <summary>
        /// Begins an operation where the mouse is left.
        /// </summary>
        public static HUMEvents.Data.Left Left(this HUMEvents.Data.Mouse mouse)
        {
            return new HUMEvents.Data.Left(mouse);
        }

        /// <summary>
        /// Begins a left mouse button action that happens twice in a row.
        /// </summary>
        public static HUMEvents.Data.DoubleLeft Left(this HUMEvents.Data.Double @double)
        {
            return new HUMEvents.Data.DoubleLeft(@double);
        }

        /// <summary>
        /// Begins an operation where the mouse is right.
        /// </summary>
        public static HUMEvents.Data.Right Right(this HUMEvents.Data.Mouse mouse)
        {
            return new HUMEvents.Data.Right(mouse);
        }

        /// <summary>
        /// Begins an operation where the mouse is top.
        /// </summary>
        public static HUMEvents.Data.Top Top(this HUMEvents.Data.Mouse mouse)
        {
            return new HUMEvents.Data.Top(mouse);
        }

        /// <summary>
        /// Begins an operation where the mouse is bottom.
        /// </summary>
        public static HUMEvents.Data.Bottom Bottom(this HUMEvents.Data.Mouse mouse)
        {
            return new HUMEvents.Data.Bottom(mouse);
        }

        public enum Side
        {
            Left,
            Right,
            Top,
            Bottom
        }
    }
}
