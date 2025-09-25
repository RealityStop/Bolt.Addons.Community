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

            if (unit.ShowSquare)
            {
                DrawUnitPosition(unitCenter, lineStart, lineEnd);
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

        private void DrawUnitPosition(Vector3 unitCenter, Vector3 lineStart, Vector3 lineEnd)
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
