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

        private static readonly List<IGraphElementWidget> hitWidgets = new List<IGraphElementWidget>();
        private static IGraphElementWidget selectedWidget;

        private const float Padding = 50f;

        public static void Draw(IGraphContext context, HashSet<IGraphElementWidget> widgets)
        {
            if (context == null || context.graph == null) return;

            var canvas = context.graph.Canvas();
            if (canvas == null) return;

            Rect rect = GUILayoutUtility.GetRect(MinimapSize.x, MinimapSize.y);
            GUI.BeginGroup(rect);

            Rect contentBounds = GraphGUI.CalculateArea(widgets);
            contentBounds.xMin -= Padding;
            contentBounds.yMin -= Padding;
            contentBounds.xMax += Padding;
            contentBounds.yMax += Padding;

            Rect viewportWorld = new Rect(canvas.pan - canvas.viewport.size * 0.5f, canvas.viewport.size);
            Rect combinedBounds = contentBounds.Encompass(viewportWorld);

            float scaleX = rect.width / combinedBounds.width;
            float scaleY = rect.height / combinedBounds.height;
            float scale = scaleX < scaleY ? scaleX : scaleY;
            if (scale <= 0f) { GUI.EndGroup(); return; }

            Vector2 minimapOffset = rect.center - combinedBounds.size * (scale * 0.5f);
            Vector2 boundsMin = combinedBounds.min;

            Vector2 ToMinimap(Vector2 worldPos)
            {
                return (worldPos - boundsMin) * scale + minimapOffset;
            }

            GraphUtility.OverrideContextIfNeeded(() =>
            {
                hitWidgets.Clear();

                var e = Event.current;
                Vector2 mousePos = e.mousePosition;
                Vector2 mouseWorld = (mousePos - minimapOffset) / scale + boundsMin;

                var selection = canvas.selection;
                bool contextIsValid = GraphContextProvider.instance.IsValid(context.reference);

                IGraphElementWidget closest = null;
                float closestDistSq = float.MaxValue;

                Rect drawRect = default;

                foreach (var w in widgets)
                {
                    if (!canvas.widgetProvider.IsValid(w.item)) continue;

                    var widget = canvas.Widget(w.item) as IGraphElementWidget;
                    if (widget == null) continue;

                    if (!contextIsValid) continue;

                    Rect wp = widget.position;

                    drawRect.position = ToMinimap(wp.position);
                    drawRect.size = wp.size * scale;

                    Handles.DrawSolidRectangleWithOutline(
                        drawRect,
                        GetElementColor(widget).WithAlpha(0.1f),
                        Color.white * (canvas.selection.Contains(widget.element) ? 1f : 0)
                    );

                    Vector2 center = wp.center;
                    Vector2 delta = center - mouseWorld;
                    float distSq = delta.sqrMagnitude;

                    if (distSq < closestDistSq)
                    {
                        closestDistSq = distSq;
                        closest = widget;
                    }

                    if (drawRect.Contains(mousePos))
                    {
                        hitWidgets.Add(widget);
                    }
                }

                if (e.type == EventType.MouseDown && e.button == 0)
                {
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
            });

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

            GUI.EndGroup();
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
            }
#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
            else if (widget is StickyNoteWidget sticky)
            {
                return StickyNote.GetStickyColor(sticky.element.colorTheme);
            }
#endif
            return Color.white;
        }
    }
}