using UnityEngine;
using UnityEditor;
using Unity.VisualScripting.Community.Libraries.Humility;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Unity.VisualScripting.Community
{
    static class Extension
    {
        public static Rect Offset(this Rect rect, float x = 0, float y = 0, float xy = 0, float w = 0, float h = 0, float wh = 0, bool centre = false)
        {
            rect.x += x + xy;
            rect.y += y + xy;
            rect.width += centre ? -(x + xy) * 2f : w + wh;
            rect.height += centre ? -(x + xy) * 2f : h + wh;

            return rect;
        }
    }

    [Widget(typeof(CommentNode))]
    public sealed class CommentNodeWidget : UnitWidget<CommentNode>
    {
        public CommentNodeWidget(FlowCanvas canvas, CommentNode unit) : base(canvas, unit) { MigrateRects(); }

        const float
            borderOutside = 12f,
            borderInside = 1.5f,
            borderText = 3f,
            borderTotal = borderOutside * 2f + borderInside * 2f + borderText * 2f;

        Rect
        wholeRect,
        borderRect,
        textRect;

        Vector2
            textAreaSize;

        int hash;

        GUIStyle
            textGUI = new GUIStyle(GUI.skin.label) { richText = true, wordWrap = true },
            titleGUI = new GUIStyle(GUI.skin.label) { richText = true, wordWrap = true, alignment = TextAnchor.MiddleLeft, fontSize = 8 };

        public override Rect position
        {
            get => unit.wholeRect; set => unit.wholeRect = value;
        }

        void MigrateRects()
        {
            if (unit.wholeRect == default && wholeRect != default)
            {
                unit.wholeRect = wholeRect;
                unit.borderRect = borderRect;
                unit.textRect = textRect;

                wholeRect = default;
                borderRect = default;
                textRect = default;
            }
        }
        
        public override void DrawBackground()
        {
            if (hash == 0) hash = unit.GetHashCode();

            // If first time running, create a palette.
            if (!CommentNodeInspector.initialised) { CommentNodeInspector.UpdatePalette(); CommentNodeInspector.initialised = true; }

            // If unit locked to palette, grab the assigned color
            if (unit.lockedToPalette)
            {
                unit.color = CommentNodeInspector.colorPalette[unit.customPalette ? 1 : 0, unit.paletteSelection.row, unit.paletteSelection.col] / 3f;
                unit.fontColor = CommentNodeInspector.fontPalette[unit.customPalette ? 2 : unit.fontColorize ? 1 : 0, unit.paletteSelection.row, unit.paletteSelection.col];
            }

            // Set text area GUI style
            textGUI.fontStyle = unit.fontBold && unit.fontItalic ? FontStyle.BoldAndItalic : unit.fontBold ? FontStyle.Bold : unit.fontItalic ? FontStyle.Italic : FontStyle.Normal;
            textGUI.fontSize = unit.fontSize;
            textGUI.alignment = unit.alignCentre ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft;

            // Get text area xy size
            textAreaSize = textGUI.CalcSizeWithConstraints(new GUIContent(unit.comment), new Vector2(Mathf.Round(unit.maxWidth - borderTotal), 1000f));
            textAreaSize.y = textGUI.CalcHeight(new GUIContent(unit.comment), unit.maxWidth - borderTotal);

            // Set whole area rect
            unit.wholeRect = new Rect(unit.position.x, unit.position.y, unit.hasTitle ? Mathf.Max(titleGUI.CalcSize(new GUIContent(unit.title)).x + borderTotal, Mathf.Clamp(textAreaSize.x + borderTotal, unit.autoWidth ? borderTotal : unit.maxWidth, unit.maxWidth)) : Mathf.Clamp(textAreaSize.x + borderTotal, unit.autoWidth ? borderTotal : unit.maxWidth, unit.maxWidth), Mathf.Clamp(textAreaSize.y + borderTotal, borderTotal, 1000));

            // Resource - https://unitylist.com/p/5c3/Unity-editor-icons

            // Draw border if mouse present
            if (unit.wholeRect.Contains(e.mousePosition) || selection.Contains(unit))
            {
                GUI.DrawTexture(unit.wholeRect, Texture2D.whiteTexture, ScaleMode.ScaleAndCrop, true, 0, unit.color * new Color(0.5f, 0.5f, 0.5f, 0.5f), 0, borderOutside);
            }

            // Draw connections to other units
            foreach (var connectedUnit in unit.connectedUnits)
            {
                if (connectedUnit == null || !(graph as FlowGraph).units.Contains(connectedUnit)) continue;
                var unitWidget = canvas.Widget(connectedUnit);
                var lineColor = unit.color;
                if (unit.curvedLine)
                {
                    Vector3 start = new Vector3(unit.position.x + unit.wholeRect.width / 2, unit.position.y + unit.wholeRect.height / 2, 0);
                    Vector3 end = new Vector3(connectedUnit.position.x + unitWidget.position.width / 2, connectedUnit.position.y + unitWidget.position.height / 2, 0);
                    Vector3 connectionEnd = new Vector2(GetEdgePosition(unitWidget.position, CompareVectors(start, end)).x, GetEdgePosition(unitWidget.position, CompareVectors(start, end)).y);
                    // Draw the connection
                    GraphGUI.DrawConnection(
                        lineColor,
                        start,
                        connectionEnd,
                        CalculateStartEnd(end, start),
                        CalculateStartEnd(start, end),
                        null,
                        Vector2.zero,
                        UnitConnectionStyles.relativeBend,
                        UnitConnectionStyles.minBend,
                        3f
                    );

                    Edge edge = CompareVectors(start, end);
                    Vector3 arrowBase = GetEdgePosition(unitWidget.position, edge);
                    DrawArrowheadAtEnd(arrowBase, edge, lineColor);
                }
                else
                {
                    Vector3 start = new Vector3(unit.position.x + unit.wholeRect.width / 2, unit.position.y + unit.wholeRect.height / 2, 0);
                    Vector3 end = new Vector3(connectedUnit.position.x + unitWidget.position.width / 2, connectedUnit.position.y + unitWidget.position.height / 2, 0);
                    Vector3 connectionEnd = new Vector2(GetEdgePosition(unitWidget.position, CompareVectors(start, end)).x, GetEdgePosition(unitWidget.position, CompareVectors(start, end)).y);
                    Vector3[] points = { start, connectionEnd };
                    Handles.color = lineColor;
                    Handles.DrawAAPolyLine(5f, points);
                    Handles.color = Color.white;
                }
            }

            // Get inner area rect
            unit.borderRect = unit.wholeRect.Offset(xy: borderOutside, centre: true);

            // Draw border
            GUI.DrawTexture(unit.borderRect, Texture2D.whiteTexture, ScaleMode.ScaleAndCrop, true, 0, unit.color, 0, 7);
            GUI.DrawTexture(unit.borderRect, Texture2D.whiteTexture, ScaleMode.ScaleAndCrop, true, 0, unit.color * (2f - unit.color.grayscale), borderInside, 7);
        }

        void DrawArrowheadAtEnd(Vector3 arrowBase, Edge edge, Color color)
        {
            float arrowLength = 10f;
            float arrowWidth = 5f;

            Vector3 direction;
            Vector3 perpendicular;

            switch (edge)
            {
                case Edge.Top:
                    direction = Vector3.down;
                    perpendicular = Vector3.left;
                    break;
                case Edge.Bottom:
                    direction = Vector3.up;
                    perpendicular = Vector3.left;
                    break;
                case Edge.Left:
                    direction = Vector3.right;
                    perpendicular = Vector3.down;
                    break;
                case Edge.Right:
                    direction = Vector3.left;
                    perpendicular = Vector3.down;
                    break;
                default:
                    throw new System.ArgumentException("Invalid edge specified for arrowhead.");
            }

            // Calculate arrowhead points (tip + two side points)
            Vector3 leftPoint = arrowBase + (direction * arrowLength) + (perpendicular * arrowWidth);
            Vector3 rightPoint = arrowBase + (direction * arrowLength) - (perpendicular * arrowWidth);

            // Create the arrowhead polygon points (tip and sides)
            Vector3[] arrowPoints = new Vector3[]
            {
            arrowBase,
            leftPoint,
            rightPoint
            };

            Handles.color = color;

            Handles.DrawAAConvexPolygon(arrowPoints);

            Handles.color = Color.white;
        }

        public Vector2 GetEdgePosition(Rect target, Edge edge)
        {
            switch (edge)
            {
                case Edge.Top:
                    return new Vector2(target.center.x, target.yMin - 3);
                case Edge.Bottom:
                    return new Vector2(target.center.x, target.yMax + 6);
                case Edge.Left:
                    return new Vector2(target.xMax + 2, target.center.y);
                case Edge.Right:
                    return new Vector2(target.xMin - 2, target.center.y);
                default:
                    throw new System.ArgumentException("Invalid edge type specified.");
            }
        }

        public Edge CompareVectors(Vector2 first, Vector2 second)
        {
            float deltaX = second.x - first.x;
            float deltaY = second.y - first.y;

            if (Mathf.Abs(deltaY) > Mathf.Abs(deltaX))
            {
                return (deltaY > 0) ? Edge.Top : Edge.Bottom;
            }
            else
            {
                return (deltaX > 0) ? Edge.Right : Edge.Left;
            }
        }

        public Edge CalculateStartEnd(Vector2 first, Vector2 second)
        {
            float deltaX = second.x - first.x;
            float deltaY = second.y - first.y;

            if (Mathf.Abs(deltaY) > Mathf.Abs(deltaX))
            {
                return (deltaY > 0) ? Edge.Top : Edge.Bottom;
            }
            else
            {
                return (deltaX > 0) ? Edge.Left : Edge.Right;
            }
        }

        public override void HandleInput()
        {
            base.HandleInput();
            if (canvas.selection.Contains(unit))
            {
                if (e.keyCode == KeyCode.C)
                {
                    metadata["connectedUnits"].RecordUndo();
                    foreach (var item in canvas.selection)
                    {
                        if (item is Unit unit && !this.unit.connectedUnits.Contains(unit) && unit != this.unit)
                        {
                            this.unit.connectedUnits.Add(unit);
                        }
                    }
                    Reposition();
                    EditorUtility.SetDirty(LudiqEditorUtility.editedObject);
                }
                else if (e.keyCode == KeyCode.X)
                {
                    metadata["connectedUnits"].RecordUndo();
                    var connectedUnits = new List<Unit>();
                    connectedUnits.AddRange(unit.connectedUnits);
                    foreach (var item in connectedUnits)
                    {
                        if (item is Unit unit && this.unit.connectedUnits.Contains(unit) && canvas.selection.Contains(unit))
                        {
                            this.unit.connectedUnits.Remove(unit);
                        }
                    }
                    Reposition();
                    EditorUtility.SetDirty(LudiqEditorUtility.editedObject);
                }
            }
        }

        protected override IEnumerable<DropdownOption> contextOptions
        {
            get
            {
                foreach (var option in base.contextOptions)
                {
                    yield return option;
                }
                foreach (var connectedUnit in unit.connectedUnits)
                {
                    Action action = () => { metadata["connectedUnits"].RecordUndo(); unit.connectedUnits.Remove(connectedUnit); EditorUtility.SetDirty(LudiqEditorUtility.editedObject); };
                    yield return new DropdownOption(action, "Disconnect " + connectedUnit);
                }
                foreach (var unit in canvas.selection.Where(element => element is Unit).Cast<Unit>())
                {
                    if (unit != this.unit)
                    {
                        Action action = () => { metadata["connectedUnits"].RecordUndo(); base.unit.connectedUnits.Add(unit); EditorUtility.SetDirty(LudiqEditorUtility.editedObject); };
                        yield return new DropdownOption(action, "Connect " + unit);
                    }
                }
            }
        }

        public override void DrawForeground()
        {
            GUI.contentColor = unit.fontColor;
            // Get text area rect
            unit.textRect = unit.borderRect.Offset(xy: borderText, centre: true);
            // If mouse hovering over unit
            if (unit.textRect.Contains(e.mousePosition))
            {
                GUI.SetNextControlName("commentField" + hash.ToString());
                EditorGUI.BeginChangeCheck();
                var comment = EditorGUI.TextArea(unit.textRect, unit.comment, textGUI);
                if (EditorGUI.EndChangeCheck())
                {
                    metadata["comment"].RecordUndo();
                    metadata["comment"].value = comment;
                }
            }
            // Draw main comment
            // If unit text selected
            else if (GUI.GetNameOfFocusedControl() == "commentField" + hash.ToString())
            {
                EditorGUI.BeginChangeCheck();
                var comment = EditorGUI.TextArea(unit.textRect, unit.comment, textGUI);
                if (EditorGUI.EndChangeCheck())
                {
                    metadata["comment"].RecordUndo();
                    metadata["comment"].value = comment;
                }
                return;
            }

            // Draw outline?
            if (unit.hasOutline)
            {
                GUI.contentColor = unit.fontColor.maxColorComponent > 0.5f ? unit.color * 0.9f * (unit.color.maxColorComponent / 1f) : (unit.color * 0.9f * (1f / unit.color.maxColorComponent)).WithAlpha(1f);

                float outline = Mathf.Max(unit.fontSize / 60f, 1f);
                EditorGUI.LabelField(unit.textRect.Offset(x: -outline, y: -outline), unit.comment, textGUI);
                EditorGUI.LabelField(unit.textRect.Offset(x: outline, y: outline), unit.comment, textGUI);
                EditorGUI.LabelField(unit.textRect.Offset(x: -outline, y: outline), unit.comment, textGUI);
                EditorGUI.LabelField(unit.textRect.Offset(x: outline, y: -outline), unit.comment, textGUI);
            }

            EditorGUI.LabelField(unit.textRect, unit.comment, textGUI);

            // Draw title
            GUI.contentColor = Color.white;
            if (unit.hasTitle)
                EditorGUI.LabelField(new Rect(unit.position.x + borderOutside + 7f, unit.position.y, unit.wholeRect.width, borderOutside), unit.title, titleGUI);

            unit.position = unit.wholeRect.position;
        }
    }
}