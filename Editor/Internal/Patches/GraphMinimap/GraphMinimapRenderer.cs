using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    internal static class MiniMapRenderer
    {
        private static Vector2 MinimapSize => new Vector2(
            GraphMiniMapStorage.Settings.width,
            GraphMiniMapStorage.Settings.height
        );

        private const float Padding = 50f;

        private static readonly Dictionary<IGraphElementWidget, Rect> cachedRects = new Dictionary<IGraphElementWidget, Rect>();
        private static readonly List<IGraphElementWidget> hitWidgets = new List<IGraphElementWidget>();
        private static IGraphElementWidget selectedWidget;
        private static double lastUpdateTime = 0;
        private const double updateInterval = 0.1;

        private static Rect contentBounds;
        private static float scale;
        private static Vector2 minimapOffset;
        private static Rect combinedBounds;

        public static void Draw(IGraphContext context, HashSet<IGraphElementWidget> widgets)
        {
            if (context == null || context.graph == null) return;

            var canvas = context.graph.Canvas();
            if (canvas == null) return;

            Rect rect = GUILayoutUtility.GetRect(MinimapSize.x, MinimapSize.y);
            GUI.BeginGroup(rect);

            double currentTime = EditorApplication.timeSinceStartup;
            bool shouldUpdate = currentTime - lastUpdateTime > updateInterval;
            lastUpdateTime = currentTime;

            if (shouldUpdate)
            {
                UpdateCache(canvas, widgets, rect);
            }

            DrawMinimap(canvas);

            HandleMouse(canvas);

            GUI.EndGroup();
        }

        private static void UpdateCache(ICanvas canvas, HashSet<IGraphElementWidget> widgets, Rect rect)
        {
            contentBounds = GraphGUI.CalculateArea(widgets);
            contentBounds.xMin -= Padding;
            contentBounds.yMin -= Padding;
            contentBounds.xMax += Padding;
            contentBounds.yMax += Padding;

            Rect viewportWorld = new Rect(canvas.pan - canvas.viewport.size * 0.5f, canvas.viewport.size);
            combinedBounds = contentBounds.Encompass(viewportWorld);

            float scaleX = rect.width / combinedBounds.width;
            float scaleY = rect.height / combinedBounds.height;
            scale = scaleX < scaleY ? scaleX : scaleY;
            if (scale <= 0f) return;

            minimapOffset = rect.center - combinedBounds.size * (scale * 0.5f);

            cachedRects.Clear();
            foreach (var w in widgets)
            {
                if (!canvas.widgetProvider.IsValid(w.item)) continue;

                var widget = canvas.Widget(w.item) as IGraphElementWidget;
                if (widget == null) continue;

                Rect wp = widget.position;
                cachedRects[widget] = new Rect(ToMinimap(wp.position), wp.size * scale);
            }
        }

        private static void DrawMinimap(ICanvas canvas)
        {
            if (scale <= 0f) return;

            foreach (var kvp in cachedRects)
            {
                var widget = kvp.Key;
                var drawRect = kvp.Value;

                Handles.DrawSolidRectangleWithOutline(
                    drawRect,
                    GetElementColor(widget).WithAlpha(0.1f),
                    Color.white * (canvas.selection.Contains(widget.element) ? 1f : 0)
                );
            }

            // Draw viewport rectangle
            Rect viewportWorld = new Rect(canvas.pan - canvas.viewport.size * 0.5f, canvas.viewport.size);
            Vector2 viewPos = ToMinimap(viewportWorld.position);
            Vector2 viewSize = viewportWorld.size * scale;

            Rect viewRect = Rect.MinMaxRect(
                Mathf.Max(viewPos.x, 0),
                Mathf.Max(viewPos.y, 0),
                Mathf.Min(viewPos.x + viewSize.x, MinimapSize.x),
                Mathf.Min(viewPos.y + viewSize.y, MinimapSize.y)
            );

            Handles.DrawSolidRectangleWithOutline(
                viewRect,
                new Color(1f, 1f, 1f, 0.1f),
                Color.yellow
            );
        }

        private static void HandleMouse(ICanvas canvas)
        {
            Event e = Event.current;
            if (e.type != EventType.MouseDown || e.button != 0) return;

            Vector2 mousePos = e.mousePosition;
            Vector2 mouseWorld = (mousePos - minimapOffset) / scale + combinedBounds.min;

            hitWidgets.Clear();
            IGraphElementWidget closest = null;
            float closestDistSq = float.MaxValue;

            foreach (var kvp in cachedRects)
            {
                var widget = kvp.Key;
                var drawRect = kvp.Value;

                if (drawRect.Contains(mousePos))
                    hitWidgets.Add(widget);

                Vector2 delta = widget.position.center - mouseWorld;
                float distSq = delta.sqrMagnitude;
                if (distSq < closestDistSq)
                {
                    closestDistSq = distSq;
                    closest = widget;
                }
            }

            IGraphElementWidget target = null;

            if (hitWidgets.Count > 0)
            {
                int index = 0;
                if (selectedWidget != null)
                {
                    int i = hitWidgets.IndexOf(selectedWidget);
                    if (i >= 0) index = (i + 1) % hitWidgets.Count;
                }
                target = hitWidgets[index];
            }
            else
            {
                target = closest;
            }

            if (target != null)
            {
                selectedWidget = target;
                canvas.ViewElements(target.element.Yield());

                var selection = canvas.selection;
                if (target.canSelect)
                {
                    if (e.shift)
                        selection.Add(target.element);
                    else
                        selection.Select(target.element);
                }

                e.Use();
            }
        }

        private static Vector2 ToMinimap(Vector2 worldPos)
        {
            return (worldPos - combinedBounds.min) * scale + minimapOffset;
        }

        private static Color GetElementColor(IWidget widget)
        {
            if (widget is IUnitWidget unitWidget)
            {
                if (unitWidget is CommentNodeWidget commentNodeWidget)
                    return commentNodeWidget.element.color;
                else if (unitWidget is ArrowWidget arrowWidget)
                    return arrowWidget.element.Color;
                else
                    return Color.gray;
            }
            else if (widget is GraphGroupWidget group)
            {
                return group.element.color;
#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
            }
            else if (widget is StickyNoteWidget sticky)
            {
                return StickyNote.GetStickyColor(sticky.element.colorTheme);
#endif
            }
            return Color.white;
        }
    }
}