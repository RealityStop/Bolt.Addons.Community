using System;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMEditor_Immediate_Children
    {
        /// <summary>
        /// Draws a rectangle that is colored and may have a border.
        /// </summary>
        public static Rect Box(this HUMEditor.Data.Immediate immediate, Color backgroundColor, Color borderColor, BorderDrawPlacement drawPlacement, float border = 1f)
        {
            Rect borderRect = new Rect();
            Rect newBackgroundRect = new Rect();

            switch (drawPlacement)
            {
                case BorderDrawPlacement.Inside:
                    borderRect = immediate.rect;

                    newBackgroundRect = new Rect(
                    immediate.rect.x + border,
                    immediate.rect.y + border,
                    immediate.rect.width - (border * 2),
                    immediate.rect.height - (border * 2)
                    );

                    break;
                case BorderDrawPlacement.Outside:
                    borderRect = new Rect(
                    immediate.rect.x - border,
                    immediate.rect.y - border,
                    immediate.rect.width + (border * 2),
                    immediate.rect.height + (border * 2)
                    );

                    newBackgroundRect = immediate.rect;
                    break;
                case BorderDrawPlacement.Centered:
                    borderRect = new Rect(
                    immediate.rect.x - (border / 2),
                    immediate.rect.y - (border / 2),
                    immediate.rect.width + (border / 2),
                    immediate.rect.height + (border / 2)
                    );

                    newBackgroundRect = new Rect(
                    immediate.rect.x + (border / 2),
                    immediate.rect.y + (border / 2),
                    immediate.rect.width - border,
                    immediate.rect.height - border
                    );
                    break;
            }

            EditorGUI.DrawRect(borderRect, borderColor);

            EditorGUI.DrawRect(newBackgroundRect, backgroundColor);

            return borderRect;
        }

        /// <summary>
        /// A colored rectangle that acts like a button.
        /// </summary>
        public static bool Box(this Data.Selection selection, ref bool IsPressed, Event e, Color normal, Color hovered, Color pressed, Action onPressed = null)
        {
            var wasPressed = false;
            var color = normal;
            selection.immediate.rect.Draw().Selection().Area(ref IsPressed, false, out wasPressed, e, () => { color = normal; }, () => { }, () => { color = pressed; });

            if (IsPressed)
            {
                EditorGUI.DrawRect(selection.immediate.rect, pressed);
                onPressed?.Invoke();
            }
            else
            {
                if (e.Mouse().Is().Inside(selection.immediate.rect))
                {
                    EditorGUI.DrawRect(selection.immediate.rect, hovered);
                }
                else
                {
                    EditorGUI.DrawRect(selection.immediate.rect, normal);
                }
            }

            return IsPressed;
        }

        /// <summary>
        /// A rect that has callbacks for when its hovered, pressed, or doing nothing.
        /// </summary>
        public static void Area(this Data.Selection selection, ref bool isBeingPressed, bool isActiveSelection, out bool wasPressed, Event e, Action normal, Action active, Action pressed)
        {
            bool justReleased = false;
            wasPressed = false;

            if (e.type == EventType.MouseDown)
            {
                isBeingPressed = true;
            }

            if (e.type == EventType.MouseUp)
            {
                if (isBeingPressed == true)
                {
                    justReleased = true;
                }

                isBeingPressed = false;
            }

            if (isActiveSelection)
            {
                active?.Invoke();
                isBeingPressed = false;
            }
            else
            {
                if (e.Mouse().Is().Inside(selection.immediate.rect))
                {
                    if (isBeingPressed)
                    {
                        pressed.Invoke();
                    }
                    else
                    {
                        if (justReleased)
                        {
                            normal.Invoke();
                            wasPressed = true;
                            isBeingPressed = false;
                        }
                        else
                        {
                            active?.Invoke();
                            isBeingPressed = false;
                        }
                    }
                }
                else
                {
                    isBeingPressed = false;
                    normal?.Invoke();
                }
            }
        }

        /// <summary>
        /// A scrollable view.
        /// </summary>
        public static Vector2 ScrollView(this HUMEditor.Data.Immediate immediate, Vector2 scrollPosition, Rect innerRect, Action contents)
        {
            var output = GUI.BeginScrollView(immediate.rect, scrollPosition, innerRect);
            contents();
            GUI.EndScrollView();
            return output;
        }

        /// <summary>
        /// A toggle that has no content and is drawn only using textures.
        /// </summary>
        public static bool Toggle(this Data.Image image, ref bool isBeingPressed, bool isActiveSelection, Event e, Texture2D normal, Texture2D active, Texture2D pressed)
        {
            Texture2D current = normal;
            bool wasPressed = false;

            image.immediate.rect.Draw().Selection().Area(ref isBeingPressed, isActiveSelection, out wasPressed, e, () => { current = normal; }, () => { current = active; }, () => { current = pressed; });

            GUI.DrawTexture(image.immediate.rect, current);

            return wasPressed;
        }

        /// <summary>
        /// A button that has no content and is drawn only using textures.
        /// </summary>
        public static bool Button(this Data.Image image, ref bool isBeingPressed, Event e, Texture2D normal, Texture2D pressed)
        {
            Texture2D current = normal;
            bool wasPressed = false;

            image.immediate.rect.Draw().Selection().Area(ref isBeingPressed, false, out wasPressed, e, () => { current = normal; }, () => { }, () => { current = pressed; });

            GUI.DrawTexture(image.immediate.rect, current);

            return wasPressed;
        }

        /// <summary>
        /// A button that has no content and is drawn only using textures.
        /// </summary>
        public static bool Button(this Data.Image image, ref bool isBeingPressed, Event e, Texture2D normal, Texture2D pressed, Texture2D hovering)
        {
            Texture2D current = normal;
            bool wasPressed = false;

            image.immediate.rect.Draw().Selection().Area(ref isBeingPressed, false, out wasPressed, e, () => { current = normal; }, () => { current = hovering; }, () => { current = pressed; });

            GUI.DrawTexture(image.immediate.rect, current);

            return wasPressed;
        }

        /// <summary>
        /// A button that has no content and is drawn only using textures.
        /// </summary>
        public static bool TintedButton(this Data.Image image, ref bool isBeingPressed, Event e, Texture2D texture, Color active, Color pressed)
        {
            Texture2D current = texture.Copy();
            bool wasPressed = false;

            image.immediate.rect.Draw().Selection().Area(ref isBeingPressed, false, out wasPressed, e, () => { }, () => { current = current.Tint(active, 0.5f); }, () => { current = current.Tint(pressed, 0.5f); });

            GUI.DrawTexture(image.immediate.rect, current);

            return wasPressed;
        }

        /// <summary>
        /// Begins the chain of Selection based controls.
        /// </summary>
        public static Data.Selection Selection(this HUMEditor.Data.Immediate draw)
        {
            return new Data.Selection(draw);
        }

        /// <summary>
        /// Begins te chain of Image based controls.
        /// </summary>
        public static Data.Image Image(this HUMEditor.Data.Immediate immediate)
        {
            return new HUMEditor_Immediate_Children.Data.Image(immediate);
        }

        /// <summary>
        /// Draws a line from one point to another, with a custom styling.
        /// </summary>
        public static void Line(this HUMEditor.Data.Render render, Vector2 startPoint, Vector2 endPoint, HUMEditor.LineStyle style)
        {
            Handles.BeginGUI();
            var col = Handles.color;
            Handles.color = style.color;
            Handles.DrawAAPolyLine(style.thickness, startPoint, endPoint);
            Handles.color = col;
            Handles.EndGUI();
        }

        /// <summary>
        /// Easily draws a zoomable and pannable canvas in one call. Includes styling options.
        /// </summary>
        public static void Canvas(this HUMEditor.Data.Immediate immediate, float zoom, Vector2 panOffset, HUMEditor.CanvasStyle canvasStyle = null)
        {
            var style = canvasStyle;
            var rect = immediate.rect;

            if (style == null) style = HUMEditor.CanvasStyle.Create();

            Vector2 center = rect.size / 2f;
            Texture2D gridTex = style.mainTexure;
            Texture2D crossTex = style.secondaryTexture;

            if (gridTex != null && crossTex != null)
            {
                // Offset from origin in tile units
                float xOffset = -(center.x * zoom + panOffset.x) / gridTex.width;
                float yOffset = ((center.y - rect.size.y) * zoom + panOffset.y) / gridTex.height;

                Vector2 tileOffset = new Vector2(xOffset, yOffset);

                // Amount of tiles
                float tileAmountX = Mathf.Round(rect.size.x * zoom) / gridTex.width;
                float tileAmountY = Mathf.Round(rect.size.y * zoom) / gridTex.height;

                Vector2 tileAmount = new Vector2(tileAmountX, tileAmountY);

                // Draw tiled background
                GUI.DrawTextureWithTexCoords(rect, gridTex, new Rect(tileOffset, tileAmount));
                GUI.DrawTextureWithTexCoords(rect, crossTex, new Rect(tileOffset + new Vector2(0.5f, 0.5f), tileAmount));
            }
        }

        /// <summary>
        /// Draws an area that can be zoomed in and out.
        /// </summary>
        public static void View(this HUMEditor.Data.Zoom zoomable, float zoom, Action action)
        {
            BeginZoomed(zoomable.rect);
            action?.Invoke();
            EndZoomed(zoomable.rect);

            void BeginZoomed(Rect pos)
            {
                GUIUtility.ScaleAroundPivot(Vector2.one / zoom, pos.size * 0.5f);
                Vector4 padding = new Vector4(0, 22, 0, 0);
                padding *= zoom;
                GUI.BeginClip(new Rect(-((pos.width * zoom) - pos.width) * 0.5f, -(((pos.height * zoom) - pos.height) * 0.5f) + (22 * zoom),
                    pos.width * zoom,
                    pos.height * zoom));
            }

            void EndZoomed(Rect pos)
            {
                GUI.EndClip();
                GUIUtility.ScaleAroundPivot(Vector2.one * zoom, pos.size * 0.5f);
                Vector3 offset = new Vector3(
                    (((pos.width * zoom) - pos.width) * 0.5f),
                    (((pos.height * zoom) - pos.height) * 0.5f) + (-22 * zoom) + 22,
                    0);
                GUI.matrix = Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one);
            }
        }

        /// <summary>
        /// Divides an area into two. Allows you to vertically adjust the area.
        /// </summary>
        public static Rect VerticalDivider(this HUMEditor.Data.Immediate immediate, Event e, Rect divider, ref Rect above, ref Rect below)
        {
            if (divider.Contains(e.mousePosition))
            {
                EditorGUIUtility.AddCursorRect(divider, MouseCursor.ResizeVertical);

                if (e.type == EventType.MouseDown)
                {
                    above = above.Add().Height(e.delta.y);
                    below = above.Add().Height(e.delta.y * -1);
                }
            }

            return new Rect(divider.x, divider.y + e.delta.y, divider.width, divider.height);
        }

        /// <summary>
        /// Divides an area into two. Allows you to horizontally adjust the area.
        /// </summary>
        public static Rect HorizontalDivider(this HUMEditor.Data.Immediate immediate, Event e, Rect divider, ref Rect before, ref Rect after)
        {
            if (divider.Contains(e.mousePosition))
            {
                EditorGUIUtility.AddCursorRect(divider, MouseCursor.ResizeHorizontal);

                if (e.type == EventType.MouseDown)
                {
                    before = before.Add().Width(e.delta.x);
                    after = after.Add().Width(e.delta.x * -1);
                }
            }

            return new Rect(divider.x + e.delta.x, divider.y, divider.width, divider.height);
        }
    }
}
