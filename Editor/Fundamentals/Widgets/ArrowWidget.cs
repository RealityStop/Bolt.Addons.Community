using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Widget(typeof(Arrow))]
    [RenamedFrom("Unity.VisualScripting.Community.ArrowNode")]
    public class ArrowWidget : UnitWidget<Arrow>
    {
        private const float arrowWidth = 10f;
        private const float arrowHeight = 10f;
        private const float lineWidth = 5f;
        private const float DottedLineWidths = 10f;
        private const float arrowHandle = 4f;

        public ArrowWidget(FlowCanvas canvas, Arrow unit) : base(canvas, unit)
        {
        }

        public override float zIndex
        {
            get
            {
                return float.MaxValue;
            }
            set { }
        }

        public override bool canClip => false;
        Vector3 lineStart;
        Vector3 lineEnd;
        public override void DrawForeground()
        {
            Vector3 unitCenter = new Vector3(position.x + position.width / 2f, position.y + position.height / 2f);
            Vector3 direction = Quaternion.Euler(0f, 0f, unit.rotationAngle) * Vector3.right;

            float totalLength = position.width + unit.Length;

            lineStart = unitCenter - direction * (totalLength / 2f);
            lineEnd = unitCenter + direction * (totalLength / 2f);
            Handles.color = unit.Color;

            switch (unit.lineType)
            {
                case LineType.Normal:
                    Handles.DrawAAPolyLine(lineWidth, lineStart, lineEnd);
                    break;
                case LineType.Dotted:
                    DrawDottedLine(lineStart, lineEnd, DottedLineWidths);
                    break;
            }

            Vector3 arrowTipStart = lineStart - (lineEnd - lineStart).normalized * arrowHeight;
            Vector3 arrowTipEnd = lineEnd + (lineEnd - lineStart).normalized * arrowHeight;

            if (unit.ShowTopArrow) DrawArrow(arrowTipStart, lineStart, unit.ArrowColor);

            if (unit.ShowBottomArrow) DrawArrow(arrowTipEnd, lineEnd, unit.ArrowColor);

            if (unit.ShowCenter)
            {
                DrawUnitPosition(unitCenter);
            }

            DrawTextField((lineStart + lineEnd) / 2f, unit.Text);

            Handles.color = Color.white;
            SendToBack();
        }

        private void DrawArrow(Vector3 arrowTip, Vector3 arrowBase, Color arrowColor)
        {
            Vector3 arrowDirection = (arrowBase - arrowTip).normalized;
            Vector3 arrowSide = Quaternion.Euler(0f, 0f, 30f) * arrowDirection;
            Vector3 arrowSide2 = Quaternion.Euler(0f, 0f, -30f) * arrowDirection;

            Vector3[] arrowPoints = new Vector3[]
            {
                arrowTip,
                arrowTip + arrowSide * arrowWidth,
                arrowTip + arrowSide2 * arrowWidth
            };

            Handles.color = arrowColor;
            Handles.DrawAAConvexPolygon(arrowPoints);
        }

        private void DrawUnitPosition(Vector3 unitCenter)
        {
            float halfSize = arrowHandle / 2f;

            if (isMouseOver)
            {
                Handles.color = Color.black;
            }
            else
            {
                Handles.color = Color.white;
            }
            Handles.DrawAAConvexPolygon(
                unitCenter + new Vector3(-halfSize, -halfSize, 0f),
                unitCenter + new Vector3(-halfSize, halfSize, 0f),
                unitCenter + new Vector3(halfSize, halfSize, 0f),
                unitCenter + new Vector3(halfSize, -halfSize, 0f)
            );
        }

        protected override IEnumerable<DropdownOption> contextOptions => base.contextOptions.Where(c => c.label != "Replace...");
        private bool _isDraggingArrow;
        private Vector2 _dragStart;
        bool isOver = false;
        public override Rect hotArea => !isOver ? base.hotArea : customHotArea;

        private Rect customHotArea;

        protected override void OnDoubleClick()
        {
            if (canvas.zoom != GraphGUI.MaxZoom)
                base.OnDoubleClick();
        }

        public override void HandleInput()
        {
            Vector2 mousePos = e.mousePosition;
            const float clickThreshold = 6f;

            bool over = false;
            bool overLine = false;

            Vector2 start = lineStart;
            Vector2 end = lineEnd;

            float distance = DistanceToLine(mousePos, start, end);
            if (distance <= clickThreshold)
                overLine = over = true;

            if (unit.ShowTopArrow && IsPointInTriangle(mousePos, GetTopArrow(start, end)))
                over = true;

            if (unit.ShowBottomArrow && IsPointInTriangle(mousePos, GetBottomArrow(start, end)))
                over = true;

            if (canvas.isSelecting)
            {
                Rect lasso = canvas.selectionArea;

                if (LineIntersectsRect(start, end, lasso))
                    over = true;

                if (unit.ShowTopArrow && TriangleIntersectsRect(GetTopArrow(start, end), lasso))
                    over = true;

                if (unit.ShowBottomArrow && TriangleIntersectsRect(GetBottomArrow(start, end), lasso))
                    over = true;
            }

            isOver = over;

            if (over)
            {
                customHotArea = position.Encompass(start).Encompass(end);

                if (unit.ShowTopArrow)
                {
                    var (a, b, c) = GetTopArrow(start, end);
                    customHotArea = customHotArea.Encompass(a).Encompass(b).Encompass(c);
                }

                if (unit.ShowBottomArrow)
                {
                    var (a, b, c) = GetBottomArrow(start, end);
                    customHotArea = customHotArea.Encompass(a).Encompass(b).Encompass(c);
                }
            }

            if (e.rawType == EventType.MouseDown && !canvas.isSelecting && over)
            {
                if (e.mouseButton == 0)
                {
                    Select();
                    GUI.changed = true;

                    if (over && e.IsMouseDrag(MouseButton.Left))
                    {
                        _isDraggingArrow = true;
                        _dragStart = mousePos;
                        e.Use();
                    }

                    if (e.clickCount == 2 && e.mouseButton == 0 && overLine)
                    {
                        Vector2 canvasCenter = canvas.pan + canvas.viewport.center;
                        Vector3 furthestEnd = Vector2.Distance(lineStart, canvasCenter) > Vector2.Distance(lineEnd, canvasCenter) ? lineStart : lineEnd;

                        GraphUtility.OverrideContextIfNeeded(() =>
                            canvas.TweenViewport(furthestEnd, GraphGUI.MaxZoom, BoltCore.Configuration.overviewSmoothing));

                        e.Use();
                    }
                    isOver = false;
                }
                else if (e.mouseButton == MouseButton.Right)
                {
                    Select();
                }
            }

            if (_isDraggingArrow && e.rawType == EventType.MouseDrag)
            {
                Vector2 delta = e.mousePosition - _dragStart;
                _dragStart = e.mousePosition;

                unit.position += delta;
                GUI.changed = true;
                e.Use();
                Reposition();
            }

            if (e.rawType == EventType.MouseUp)
            {
                _isDraggingArrow = false;
            }

            if (e.clickCount == 2 && e.mouseButton == MouseButton.Left)
            {
                if (unit.ShowTopArrow && IsPointInTriangle(mousePos, GetTopArrow(start, end)))
                {
                    GraphUtility.OverrideContextIfNeeded(() => canvas.TweenViewport(start, GraphGUI.MaxZoom, BoltCore.Configuration.overviewSmoothing));
                }

                if (unit.ShowBottomArrow && IsPointInTriangle(mousePos, GetBottomArrow(start, end)))
                {
                    GraphUtility.OverrideContextIfNeeded(() => canvas.TweenViewport(end, GraphGUI.MaxZoom, BoltCore.Configuration.overviewSmoothing));
                }
            }

            base.HandleInput();
        }

        private void Select()
        {
            if (e.shift || e.ctrlOrCmd)
            {
                selection.Add(unit);
            }
            else
            {
                selection.Select(unit);
            }
        }

        private static (Vector3, Vector3, Vector3) GetTopArrow(Vector3 start, Vector3 end)
        {
            Vector3 dir = (end - start).normalized;
            return (
                start,
                start - dir * 10f + Quaternion.Euler(0, 0, 30f) * -dir * 10f,
                start - dir * 10f + Quaternion.Euler(0, 0, -30f) * -dir * 10f
            );
        }

        private static (Vector3, Vector3, Vector3) GetBottomArrow(Vector3 start, Vector3 end)
        {
            Vector3 dir = (end - start).normalized;
            return (
                end,
                end + dir * 10f + Quaternion.Euler(0, 0, 30f) * dir * 10f,
                end + dir * 10f + Quaternion.Euler(0, 0, -30f) * dir * 10f
            );
        }

        private static bool IsPointInTriangle(Vector2 p, (Vector2 a, Vector2 b, Vector2 c) tri)
        {
            static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
                => (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);

            bool b1 = Sign(p, tri.a, tri.b) < 0.0f;
            bool b2 = Sign(p, tri.b, tri.c) < 0.0f;
            bool b3 = Sign(p, tri.c, tri.a) < 0.0f;

            return (b1 == b2) && (b2 == b3);
        }

        private static bool LineIntersectsRect(Vector2 a, Vector2 b, Rect r)
        {
            if (r.Contains(a) || r.Contains(b))
                return true;

            Vector2[] corners = {
                new Vector2(r.xMin, r.yMin),
                new Vector2(r.xMax, r.yMin),
                new Vector2(r.xMax, r.yMax),
                new Vector2(r.xMin, r.yMax)
            };

            for (int i = 0; i < 4; i++)
            {
                if (LinesIntersect(a, b, corners[i], corners[(i + 1) % 4]))
                    return true;
            }

            return false;
        }

        private static bool LinesIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            float d = (a2.x - a1.x) * (b2.y - b1.y) - (a2.y - a1.y) * (b2.x - b1.x);
            if (Mathf.Abs(d) < Mathf.Epsilon) return false;

            float u = ((b1.x - a1.x) * (b2.y - b1.y) - (b1.y - a1.y) * (b2.x - b1.x)) / d;
            float v = ((b1.x - a1.x) * (a2.y - a1.y) - (b1.y - a1.y) * (a2.x - a1.x)) / d;

            return (u >= 0 && u <= 1 && v >= 0 && v <= 1);
        }

        private static bool TriangleIntersectsRect((Vector2 a, Vector2 b, Vector2 c) tri, Rect r)
        {
            if (r.Contains(tri.a) || r.Contains(tri.b) || r.Contains(tri.c))
                return true;

            return LineIntersectsRect(tri.a, tri.b, r)
                || LineIntersectsRect(tri.b, tri.c, r)
                || LineIntersectsRect(tri.c, tri.a, r);
        }

        private static float DistanceToLine(Vector2 point, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            Vector2 ap = point - a;
            float t = Mathf.Clamp01(Vector2.Dot(ap, ab) / ab.sqrMagnitude);
            Vector2 closest = a + t * ab;
            return Vector2.Distance(point, closest);
        }

        private void DrawTextField(Vector3 position, string text)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;

            Vector2 screenPos = HandleUtility.WorldToGUIPoint(position);

            GUIContent content = new GUIContent(text);
            Vector2 textSize = style.CalcSize(content);

            Rect labelRect = new Rect(screenPos.x - textSize.x / 2f, screenPos.y - textSize.y / 2f, textSize.x, textSize.y);

            Handles.BeginGUI();
            GUI.Label(labelRect, content, style);
            Handles.EndGUI();
        }

        private void DrawDottedLine(Vector3 start, Vector3 end, float segmentLength, float gapLength = 5f)
        {
            Vector3 direction = (end - start).normalized;

            Vector3 currentPosition = start;

            while (Vector3.Distance(currentPosition, end) > segmentLength)
            {
                Vector3 segmentEnd = currentPosition + direction * segmentLength;
                Handles.DrawAAPolyLine(currentPosition, segmentEnd);
                currentPosition = segmentEnd + direction * gapLength;
            }

            if (Vector3.Distance(currentPosition, end) > 0f)
            {
                Handles.DrawAAPolyLine(currentPosition, end);
            }
        }
    }
}
