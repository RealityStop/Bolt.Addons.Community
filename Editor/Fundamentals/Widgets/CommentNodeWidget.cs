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
            titleGUI = new GUIStyle(GUI.skin.label) { richText = true, wordWrap = true, alignment = TextAnchor.MiddleLeft, fontSize = 10 };

        public override Rect position
        {
            get => unit.wholeRect; set => unit.wholeRect = value;
        }

        public override bool canClip => false;

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

            List<int> invalidIndexs = new List<int>();
            int index = 0;
            // Draw connections to other units
            foreach (var connectedElement in unit.connectedElements)
            {
                if (connectedElement == null || !canvas.widgetProvider.IsValid(connectedElement))
                {
                    invalidIndexs.Add(index);
                    continue;
                }
                var elementWidget = canvas.Widget(connectedElement);
                var lineColor = unit.color;
                if (unit.curvedLine)
                {
                    Vector3 start = new Vector3(unit.position.x + unit.wholeRect.width / 2, unit.position.y + unit.wholeRect.height / 2, 0);
                    Vector3 end = GetElementPosition(connectedElement, elementWidget);
                    var targetEdge = CompareVectors(start, end);
                    Vector3 connectionEnd = CorrectLineEnd(targetEdge, new Vector2(GetEdgePosition(elementWidget.position, targetEdge, connectedElement).x, GetEdgePosition(elementWidget.position, targetEdge, connectedElement).y));
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
                    Vector3 arrowBase = GetEdgePosition(elementWidget.position, edge, connectedElement);
                    DrawArrowheadAtEnd(arrowBase, edge, lineColor);
                }
                else
                {
                    Vector3 start = new Vector3(unit.position.x + unit.wholeRect.width / 2, unit.position.y + unit.wholeRect.height / 2, 0);
                    Vector3 end = GetElementPosition(connectedElement, elementWidget);
                    var targetEdge = CompareVectors(start, end);
                    Vector3 connectionEnd = CorrectLineEnd(targetEdge, new Vector2(GetEdgePosition(elementWidget.position, targetEdge, connectedElement).x, GetEdgePosition(elementWidget.position, targetEdge, connectedElement).y));
                    Vector3[] points = { start, connectionEnd };
                    Handles.color = lineColor;
                    Handles.DrawAAPolyLine(5f, points);
                    Handles.color = Color.white;
                    Edge edge = CompareVectors(start, end);
                    Vector3 arrowBase = GetEdgePosition(elementWidget.position, edge, connectedElement);
                    DrawArrowheadAtEnd(arrowBase, edge, lineColor);
                }
                index++;
            }

            foreach (var _index in invalidIndexs)
            {
                unit.connectedElements.RemoveAt(_index);
            }

            // Get inner area rect
            unit.borderRect = unit.wholeRect.Offset(xy: borderOutside, centre: true);

            // Draw border
            GUI.DrawTexture(unit.borderRect, Texture2D.whiteTexture, ScaleMode.ScaleAndCrop, true, 0, unit.color, 0, 7);
            GUI.DrawTexture(unit.borderRect, Texture2D.whiteTexture, ScaleMode.ScaleAndCrop, true, 0, unit.color * (2f - unit.color.grayscale), borderInside, 7);
        }
        private Vector2 GetElementPosition(IGraphElement graphElement, IGraphElementWidget elementWidget)
        {
            if (graphElement is GraphGroup graphGroup)
            {
                return new Vector2(graphGroup.position.position.x + elementWidget.position.width / 2, graphGroup.position.position.y + elementWidget.position.height / 2);
            }
#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
            else if (graphElement is StickyNote stickyNote)
            {
                return new Vector2(stickyNote.position.position.x + elementWidget.position.width / 2, stickyNote.position.position.y + elementWidget.position.height / 2);
            }
#endif
            else if (graphElement is Unit unit)
            {
                return new Vector2(unit.position.x + elementWidget.position.width / 2, unit.position.y + (elementWidget.position.height / 2));
            }

            throw new InvalidOperationException("Cannot get element position : " + graphElement);
        }

        Vector2 CorrectLineEnd(Edge edge, Vector2 vector2)
        {
            if (edge == Edge.Top)
            {
                return new Vector2(vector2.x, vector2.y - 10);
            }
            else if (edge == Edge.Bottom)
            {
                return new Vector2(vector2.x, vector2.y + 10);
            }
            else if (edge == Edge.Right)
            {
                return new Vector2(vector2.x - 10, vector2.y);
            }
            else
            {
                return new Vector2(vector2.x + 10, vector2.y);
            }
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
#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
        public Vector2 GetEdgePosition(Rect target, Edge edge, IGraphElement graphElement)
        {
            switch (edge)
            {
                case Edge.Top:
                    {
                        if (graphElement is GraphGroup or StickyNote)
                        {
                            return new Vector2(target.center.x, target.yMin);
                        }
                        else if (graphElement is CommentNode)
                        {
                            return new Vector2(target.center.x, target.yMin);
                        }
                        return new Vector2(target.center.x, target.yMin - 3);
                    }
                case Edge.Bottom:
                    {
                        if (graphElement is GraphGroup or StickyNote)
                        {
                            new Vector2(target.center.x, target.yMax);
                        }
                        else if (graphElement is CommentNode)
                        {
                            return new Vector2(target.center.x, target.yMax);
                        }
                        return new Vector2(target.center.x, target.yMax + 6);
                    }
                case Edge.Left:
                    if (graphElement is GraphGroup)
                    {
                        return new Vector2(target.xMax, target.center.y);
                    }
                    else if (graphElement is CommentNode)
                    {
                        return new Vector2(target.xMax, target.yMin + 30);
                    }
                    return new Vector2(target.xMax + 2, target.yMin + 30);
                case Edge.Right:
                    if (graphElement is GraphGroup)
                    {
                        return new Vector2(target.xMin, target.center.y);
                    }
                    else if (graphElement is CommentNode)
                    {
                        return new Vector2(target.xMin, target.yMin + 30);
                    }
                    return new Vector2(target.xMin - 2, target.yMin + 30);
                default:
                    throw new System.ArgumentException("Invalid edge type specified.");
            }
        }
#else
        public Vector2 GetEdgePosition(Rect target, Edge edge, IGraphElement graphElement)
        {
            switch (edge)
            {
                case Edge.Top:
                    {
                        if (graphElement is GraphGroup)
                        {
                            return new Vector2(target.center.x, target.yMin);
                        }
                        else if (graphElement is CommentNode)
                        {
                            return new Vector2(target.center.x, target.yMin);
                        }
                        return new Vector2(target.center.x, target.yMin - 3);
                    }
                case Edge.Bottom:
                    {
                        if (graphElement is GraphGroup)
                        {
                            new Vector2(target.center.x, target.yMax);
                        }
                        else if (graphElement is CommentNode)
                        {
                            return new Vector2(target.center.x, target.yMax);
                        }
                        return new Vector2(target.center.x, target.yMax + 6);
                    }
                case Edge.Left:
                    if (graphElement is GraphGroup)
                    {
                        return new Vector2(target.xMax, target.center.y);
                    }
                    else if (graphElement is CommentNode)
                    {
                        return new Vector2(target.xMax, target.yMin + 30);
                    }
                    return new Vector2(target.xMax + 2, target.yMin + 30);
                case Edge.Right:
                    if (graphElement is GraphGroup)
                    {
                        return new Vector2(target.xMin, target.center.y);
                    }
                    else if (graphElement is CommentNode)
                    {
                        return new Vector2(target.xMin, target.yMin + 30);
                    }
                    return new Vector2(target.xMin - 2, target.yMin + 30);
                default:
                    throw new System.ArgumentException("Invalid edge type specified.");
            }
        }
#endif
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
                    metadata["connectedElements"].RecordUndo();
                    foreach (var element in canvas.selection)
                    {
                        if (!unit.connectedElements.Contains(element) && element != unit)
                        {
                            unit.connectedElements.Add(element);
                        }
                    }
                    Reposition();
                }
                else if (e.keyCode == KeyCode.X)
                {
                    metadata["connectedElements"].RecordUndo();
                    var connectedElements = new List<IGraphElement>();
                    connectedElements.AddRange(unit.connectedElements);
                    foreach (var element in connectedElements)
                    {
                        if (unit.connectedElements.Contains(element) && canvas.selection.Contains(element))
                        {
                            unit.connectedElements.Remove(element);
                        }
                    }
                    Reposition();
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
                foreach (var connectedUnit in unit.connectedElements)
                {
                    Action action = () => { metadata["connectedElements"].RecordUndo(); unit.connectedElements.Remove(connectedUnit); };
                    yield return new DropdownOption(action, "Disconnect " + connectedUnit);
                }
                foreach (var element in canvas.selection.Where(element => element is IGraphElement))
                {
                    if (element != unit)
                    {
                        Action action = () => { metadata["connectedElements"].RecordUndo(); unit.connectedElements.Add(element); };
                        yield return new DropdownOption(action, "Connect " + element);
                    }
                }
            }
        }

        public override void DrawForeground()
        {
            GUI.contentColor = Color.white;
            if (unit.hasTitle)
                EditorGUI.LabelField(new Rect(unit.position.x + borderOutside + 7f, unit.position.y, unit.wholeRect.width, borderOutside), unit.title, titleGUI);

            GUI.contentColor = unit.fontColor;

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
                return;
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
                GUI.contentColor = Color.white;
                return;
            }

            // Draw outline?
            if (unit.hasOutline)
            {
                GUI.contentColor = unit.fontColor.maxColorComponent > 0.5f ? unit.color * 0.9f * (unit.color.maxColorComponent / 1f) : (unit.color * 0.9f * (1f / unit.color.maxColorComponent)).WithAlpha(1f);

                float outline = Mathf.Max(unit.fontSize / 60f, 1f);
                EditorGUI.LabelField(unit.textRect.Offset(x: -outline, y: -outline), unit.comment, new GUIStyle(textGUI) { fontSize = unit.fontSize + 1 });
                EditorGUI.LabelField(unit.textRect.Offset(x: outline, y: outline), unit.comment, new GUIStyle(textGUI) { fontSize = unit.fontSize + 1 });
                EditorGUI.LabelField(unit.textRect.Offset(x: -outline, y: outline), unit.comment, new GUIStyle(textGUI) { fontSize = unit.fontSize + 1 });
                EditorGUI.LabelField(unit.textRect.Offset(x: outline, y: -outline), unit.comment, new GUIStyle(textGUI) { fontSize = unit.fontSize + 1 });
                GUI.contentColor = unit.fontColor;
            }

            EditorGUI.LabelField(unit.textRect, unit.comment, textGUI);
            GUI.contentColor = Color.white;

            unit.position = unit.wholeRect.position;
        }
    }
}