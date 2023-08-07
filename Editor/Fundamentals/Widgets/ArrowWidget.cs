using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Widget(typeof(Arrow))]
    public class DividerNode : UnitWidget<Arrow>
    {
        private const float arrowWidth = 10f;
        private const float arrowHeight = 10f;
        private const float lineWidth = 5f; // Adjust this value to control the line width
        private const float DottedLineWidths = 4f;
        private const float unitSquareSize = 4f; // Adjust this value to control the size of the unit square

        public DividerNode(FlowCanvas canvas, Arrow unit) : base(canvas, unit)
        {
        }

        public override void DrawForeground()
        {
            
            // Find the center of the unit
            Vector3 unitCenter = new Vector3(position.x + position.width / 2f, position.y + position.height / 2f);

            // Calculate the line's start and end points based on the unit's center and size
            float halfWidth = position.width / 2f;
            float halfHeight = position.height / 2f;
            float lineHalfWidth = halfWidth - unitSquareSize / 2f;

            // Get the line length from the unit's Length property
            float lineLength = Mathf.Max(unit.Length, 0f);

            // Calculate the rotation angle in radians
            float rotationAngleRad = unit.rotationAngle * Mathf.Deg2Rad;

            Vector3 lineStart = unitCenter + Quaternion.Euler(0f, 0f, unit.rotationAngle) * new Vector3(-lineHalfWidth, 0f, 0f);
            Vector3 lineEnd = unitCenter + Quaternion.Euler(0f, 0f, unit.rotationAngle) * new Vector3(lineHalfWidth + lineLength, 0f, 0f);

            // Draw the line with rotation based on unit.rotationAngle
            Handles.color = unit.Color;

            // Check the line type and draw the appropriate line
            switch (unit.lineType)
            {
                case LineType.Normal:
                    Handles.DrawAAPolyLine(lineWidth, lineStart, lineEnd);
                    break;
                case LineType.Dotted:
                    DrawDottedLine(lineStart, lineEnd, DottedLineWidths);
                    break;
            }
            
            // Draw arrows at the start and end of the line
            Vector3 arrowTipStart = lineStart - (lineEnd - lineStart).normalized * arrowHeight;
            Vector3 arrowTipEnd = lineEnd + (lineEnd - lineStart).normalized * arrowHeight;

            if(unit.ShowTopArrow) DrawArrow(arrowTipStart, lineStart, unit.ArrowColor);

            if (unit.ShowBottomArrow) DrawArrow(arrowTipEnd, lineEnd, unit.ArrowColor);

            // Draw the square at the unit's position
            if (unit.ShowSquare)
            {
                DrawUnitPosition(unitCenter);
            }

            // Draw the text field in the middle of the line
            DrawTextField((lineStart + lineEnd) / 2f, unit.Text);

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
            // Calculate the half-size of the unit square
            float halfSize = unitSquareSize / 2f;

            // Draw the square at the unit's position
            if (!isMouseOver)
            {
                Handles.color = Color.white;
            }
            else
            {
                Handles.color = Color.black;
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
            // Define the style for the text label
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;

            // Convert world position to screen space
            Vector2 screenPos = HandleUtility.WorldToGUIPoint(position);

            // Calculate the size of the label based on the text length
            GUIContent content = new GUIContent(text);
            Vector2 textSize = style.CalcSize(content);

            // Calculate the position for the label in screen space
            Rect labelRect = new Rect(screenPos.x - textSize.x / 2f, screenPos.y - textSize.y / 2f, textSize.x, textSize.y);

            // Draw the text label at the calculated position
            Handles.BeginGUI();
            GUI.Label(labelRect, content, style);
            Handles.EndGUI();
        }

        private void DrawDottedLine(Vector3 start, Vector3 end, float width)
        {
            // Draw the dotted line using Handles.DrawDottedLine
            Handles.DrawDottedLine(start, end, width);
        }
    }
}
