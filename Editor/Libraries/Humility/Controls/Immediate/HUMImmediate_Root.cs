using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMEditor
    {
        public static bool Changed(Action actionToCheck = null, Action onChanged = null, Action onNotChanged = null)
        {
            EditorGUI.BeginChangeCheck();
            actionToCheck?.Invoke();
            if (EditorGUI.EndChangeCheck())
            {
                onChanged?.Invoke();
                return true;
            }
            else
            {
                onNotChanged?.Invoke();
            }

            return false;
        }

        public static void Block(this Metadata metadata, Action check, Action changed, bool recordUndo = true, Rect position = new Rect())
        {
            Inspector.BeginBlock(metadata, position);
            check();
            if (Inspector.EndBlock(metadata))
            {
                changed();
                if (recordUndo) metadata.RecordUndo();
            }
        }

        public static void Disabled(bool isDisabled, Action action)
        {
            EditorGUI.BeginDisabledGroup(isDisabled);
            action();
            EditorGUI.EndDisabledGroup();
        }

        public static Data.Render Render()
        {
            return new Data.Render();
        }

        public static Data.Zoom Zoom(this Rect rect)
        {
            return new Data.Zoom(rect);
        }

        /// <summary>
        /// Starts an Immediate mode drawing of a control. A rect must be supplied for its area.
        /// </summary>
        public static Data.Immediate Draw(this Rect rect)
        {

            return new Data.Immediate(rect);
        }

        /// <summary>
        /// Modifies the zoom level based on the mouse scroll wheel
        /// </summary>
        public static float Zoom(Event e, float currentZoomLevel)
        {
            var zoom = currentZoomLevel;

            if (e.isScrollWheel)
            {
                if (e.delta.y > 0)
                {
                    zoom += 0.25f;
                }
                else
                {
                    if (e.delta.y < 0)
                    {
                        zoom -= 0.25f;
                    }
                }
            }

            return Mathf.Clamp(zoom, 1, 5);
        }

        public static void Zoom(Event e, ref float currentZoomLevel)
        {
            currentZoomLevel = Zoom(e, currentZoomLevel);
        }

        public static bool Drag(Event e, float zoom, ref Vector2 panOffset)
        {
            if (e.button == 2 && e.type == EventType.MouseDrag)
            {
                panOffset += e.delta * zoom;
                return true;
            }

            return false;
        }
    }
}
