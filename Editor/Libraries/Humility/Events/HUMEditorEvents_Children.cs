using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMEditorEvents_Children
    {
        private static Rect Resizer(HUMEvents.Data.Mouse mouse, Rect handleRect, Rect rect, Vector2 minSize, Vector2 maxSize, HUMEvents_Children.Side side, bool mouseDown, ref bool isUsing)
        {
            var mousePosition = EditorGUIUtility.GUIToScreenPoint(mouse.e.mousePosition);
            var delta = HUMEvents.mouseDelta;
            var handleContains = handleRect.Contains(mousePosition);

            if (mouse.Left().Clicked() && handleContains)
            {
                isUsing = true;
                HUMEvents_Children.lastLeftClickPosition = mousePosition;
            }

            if (mouse.Left().Released())
            {
                isUsing = false;
            }

            if (handleContains && !mouseDown)
            {

                if (side == HUMEvents_Children.Side.Bottom || side == HUMEvents_Children.Side.Top)
                {
                    EditorGUIUtility.AddCursorRect(new Rect(mouse.e.mousePosition, new Vector2(34, 34)), MouseCursor.ResizeVertical);
                }
                else
                {
                    EditorGUIUtility.AddCursorRect(new Rect(mouse.e.mousePosition, new Vector2(34, 34)), MouseCursor.ResizeHorizontal);
                }
            }

            if (!mouseDown) { HUMEvents_Children.lastLeftClickPosition = mousePosition; }
            else
            {
                if (isUsing)
                {
                    if (side == HUMEvents_Children.Side.Bottom || side == HUMEvents_Children.Side.Top)
                    {
                        EditorGUIUtility.AddCursorRect(new Rect(mouse.e.mousePosition, new Vector2(34, 34)), MouseCursor.ResizeVertical);
                    }
                    else
                    {
                        EditorGUIUtility.AddCursorRect(new Rect(mouse.e.mousePosition, new Vector2(34, 34)), MouseCursor.ResizeHorizontal);
                    }

                    var yDeltaMax = (mousePosition.y - rect.y) - (rect.yMax - rect.y);
                    var xDeltaMax = (mousePosition.x - rect.x) - (rect.xMax - rect.x);
                    var yDeltaMin = (mousePosition.y - rect.y) - (rect.yMin - rect.y);
                    var xDeltaMin = (mousePosition.x - rect.x) - (rect.xMin - rect.x);

                    if (mouse.e.type == EventType.MouseDrag)
                    {
                        if (side == HUMEvents_Children.Side.Bottom)
                        {
                            if ((rect.height + yDeltaMax < maxSize.y && rect.height + yDeltaMax > minSize.y) ||
                                yDeltaMax > 0)
                            {
                                mouse.e.Use();
                                return rect.Add().Height(yDeltaMax);
                            }
                        }

                        if (side == HUMEvents_Children.Side.Top)
                        {
                            if ((rect.height + yDeltaMin < maxSize.y && rect.height + yDeltaMin > minSize.y) ||
                                yDeltaMin < 0)
                            {
                                mouse.e.Use();
                                return rect.Subtract().Height(yDeltaMin).Add().Y(yDeltaMin);
                            }
                        }

                        if (side == HUMEvents_Children.Side.Left)
                        {
                            if ((rect.width + xDeltaMin < maxSize.x && rect.width + xDeltaMin > minSize.x) ||
                                xDeltaMin < 0)
                            {
                                mouse.e.Use();
                                return rect.Subtract().Width(xDeltaMin).Add().X(xDeltaMin);
                            }
                        }

                        if (side == HUMEvents_Children.Side.Right)
                        {
                            if ((rect.width + xDeltaMax < maxSize.x && rect.width + xDeltaMax > minSize.x) ||
                                xDeltaMax > 0)
                            {
                                mouse.e.Use();
                                return rect.Add().Width(xDeltaMax);
                            }
                        }

                        mouse.e.Use();
                    }
                }
            }

            return rect;
        }
        /// <summary>
        /// A vertical handle for manipulating a rects size. 
        /// Changes the cursor when inside the top of rect. 
        /// Clicking and holding the left mouse will return a new value based on the mouse movement.
        /// </summary>
        public static Rect Resizer(this HUMEvents.Data.Top top, Rect rect, Vector2 minSize, Vector2 maxSize, float handleHeight, bool mouseDown, ref bool isUsing)
        {
            var handleRect = new Rect(rect.x, rect.y - (handleHeight / 2), rect.width, handleHeight);
            return Resizer(top.mouse, handleRect, rect, minSize, maxSize, HUMEvents_Children.Side.Top, mouseDown, ref isUsing);
        }

        /// <summary>
        /// A vertical handle for manipulating a rects size. 
        /// Changes the cursor when inside the bottom of rect. 
        /// Clicking and holding the left mouse will return a new value based on the mouse movement.
        /// </summary>
        public static Rect Resizer(this HUMEvents.Data.Bottom bottom, Rect rect, Vector2 minSize, Vector2 maxSize, float handleHeight, bool mouseDown, ref bool isUsing)
        {
            var handleRect = new Rect(rect.x, rect.yMax - (handleHeight / 2), rect.width, handleHeight);
            return Resizer(bottom.mouse, handleRect, rect, minSize, maxSize, HUMEvents_Children.Side.Bottom, mouseDown, ref isUsing);
        }

        /// <summary>
        /// A horizontal handle for manipulating a rects size. 
        /// Changes the cursor when inside the left of rect. 
        /// Clicking and holding the left mouse will return a new value based on the mouse movement.
        /// </summary>
        public static Rect Resizer(this HUMEvents.Data.Left left, Rect rect, Vector2 minSize, Vector2 maxSize, float handleWidth, bool mouseDown, ref bool isUsing)
        {
            var handleRect = new Rect(rect.x - (handleWidth / 2), rect.y, handleWidth, rect.height);
            return Resizer(left.mouse, handleRect, rect, minSize, maxSize, HUMEvents_Children.Side.Left, mouseDown, ref isUsing);
        }

        /// <summary>
        /// A horizontal handle for manipulating a rects size. 
        /// Changes the cursor when inside the right of rect. 
        /// Clicking and holding the left mouse will return a new value based on the mouse movement.
        /// </summary>
        public static Rect Resizer(this HUMEvents.Data.Right right, Rect rect, Vector2 minSize, Vector2 maxSize, float handleWidth, bool mouseDown, ref bool isUsing)
        {
            var handleRect = new Rect(rect.xMax - (handleWidth / 2), rect.y, handleWidth, rect.height);
            return Resizer(right.mouse, handleRect, rect, minSize, maxSize, HUMEvents_Children.Side.Right, mouseDown, ref isUsing);
        }
    }
}
