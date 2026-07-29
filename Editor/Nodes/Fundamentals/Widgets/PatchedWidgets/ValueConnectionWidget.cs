using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community
{
    public sealed class ValueConnectionWidget : UnitConnectionWidget<ValueConnection>
    {
        public ValueConnectionWidget(FlowCanvas canvas, ValueConnection connection) : base(canvas, connection) { }

        private new ValueConnection.DebugData ConnectionDebugData => GetDebugData<ValueConnection.DebugData>();

        private static readonly Dictionary<Type, List<MemberInfo>> _inspectableMembersCache = new Dictionary<Type, List<MemberInfo>>();

        #region Drawing

        public override Color color => DetermineColor(connection.source.type, connection.destination.type);

        protected override bool colorIfActive => !BoltFlow.Configuration.animateControlConnections || !BoltFlow.Configuration.animateValueConnections;
        public bool hideConnection { get; private set; }
        public override void DrawForeground()
        {
            base.DrawForeground();

            if (BoltFlow.Configuration.showConnectionValues && !hideConnection)
            {
                var showLastValue = EditorApplication.isPlaying && ConnectionDebugData.assignedLastValue;
                var showPredictedvalue = BoltFlow.Configuration.predictConnectionValues && !EditorApplication.isPlaying && Flow.CanPredict(connection.source, reference);

                if (showLastValue || showPredictedvalue)
                {
                    var previousIconSize = EditorGUIUtility.GetIconSize();
                    EditorGUIUtility.SetIconSize(new Vector2(IconSize.Small, IconSize.Small));

                    object value;

                    if (showLastValue)
                    {
                        value = ConnectionDebugData.lastValue;
                    }
                    else // if (showPredictedvalue)
                    {
                        try
                        {
                            value = Flow.Predict(connection.source, reference);
                        }
                        catch (Exception ex)
                        {
                            value = ex;
                        }
                    }

                    var valueString = value.ToShortString();
                    if (value is Exception exception)
                    {
                        var label = EditorGUIUtility.TrTextContentWithIcon("", MessageType.Error);
                        var labelSize = Styles.prediction.CalcSize(label);
                        var labelPosition = new Rect(position.position - labelSize / 2, labelSize);
                        BeginDim();
                        GUI.Label(labelPosition, label, Styles.prediction);
                        EndDim();
                        if (labelPosition.Contains(Event.current.mousePosition))
                        {
                            var innerRect = new Rect(labelPosition.x + 1, labelPosition.y + 1, labelPosition.width - 2, labelPosition.height - 2);

                            var errorText = exception.Message;
                            var textSize = Styles.prediction.CalcSize(new GUIContent(errorText));
                            var textRect = new Rect(labelPosition.x + labelPosition.width + 6, labelPosition.center.y - textSize.y / 2, textSize.x, textSize.y);

                            GUI.Label(textRect, errorText, Styles.prediction);
                        }
                    }
                    else if (value is Color colorValue)
                    {
                        var label = new GUIContent(valueString, Icons.Type(typeof(Color))?[IconSize.Small]);
                        var labelSize = Styles.prediction.CalcSize(label);
                        var labelPosition = new Rect(position.position - labelSize / 2, labelSize);

                        BeginDim();
                        GUI.Label(labelPosition, label, Styles.prediction);
                        EndDim();

                        if (labelPosition.Contains(Event.current.mousePosition))
                        {
                            const float size = 20f;
                            var rect = new Rect(labelPosition.xMax + 6, labelPosition.center.y - size / 2, size, size);

                            EditorGUI.DrawRect(rect, Color.black);
                            var innerRect = new Rect(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2);
                            EditorGUI.DrawRect(innerRect, colorValue);

                            var colorText = $"RGBA({colorValue.r:F2}, {colorValue.g:F2}, {colorValue.b:F2}, {colorValue.a:F2})";
                            var textSize = Styles.prediction.CalcSize(new GUIContent(colorText));
                            var textRect = new Rect(rect.x + rect.width + 6, rect.center.y - textSize.y / 2, textSize.x, textSize.y);

                            GUI.Label(textRect, colorText, Styles.prediction);
                        }
                    }
                    else if (string.IsNullOrEmpty(valueString))
                    {
                        var type = value.GetType();
                        var label = new GUIContent(valueString, Icons.Type(type)?[IconSize.Small]);
                        var labelSize = Styles.prediction.CalcSize(label);
                        var labelPosition = new Rect(position.position - labelSize / 2, labelSize);

                        BeginDim();
                        GUI.Label(labelPosition, label, Styles.prediction);
                        EndDim();

                        if (labelPosition.Contains(Event.current.mousePosition))
                        {
                            if (!_inspectableMembersCache.TryGetValue(type, out var inspectableMembers))
                            {
                                inspectableMembers = type
                                    .GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                    .Where(m =>
                                    {
                                        if (m is FieldInfo f) return f.IsPublic || f.HasAttribute<SerializeField>() || f.HasAttribute<SerializeAttribute>() || f.HasAttribute<InspectableAttribute>();
                                        if (m is PropertyInfo p) return (p.HasAttribute<InspectableAttribute>() || p.HasAttribute<SerializeAttribute>() || p.IsPubliclyGettable()) && p.CanRead;
                                        return false;
                                    })
                                    .ToList();

                                _inspectableMembersCache[type] = inspectableMembers;
                            }

                            if (inspectableMembers.Count > 0)
                            {
                                float maxWidth = 0;
                                float lineHeight = 16f;
                                string[] lines = new string[inspectableMembers.Count];

                                for (int i = 0; i < inspectableMembers.Count; i++)
                                {
                                    var member = inspectableMembers[i];
                                    object memberValue = null;

                                    string str;
                                    try
                                    {
                                        if (member is FieldInfo fi)
                                            memberValue = fi.GetValueOptimized(value);
                                        else if (member is PropertyInfo pi && pi.CanRead)
                                            memberValue = pi.GetValueOptimized(value);

                                        str = $"{member.Name}: {memberValue?.ToString() ?? "null"}";
                                    }
                                    catch
                                    {
                                        str = $"{member.Name}: Unknown";
                                    }

                                    lines[i] = str;

                                    var size = Styles.prediction.CalcSize(new GUIContent(str));
                                    maxWidth = Mathf.Max(maxWidth, size.x);
                                }

                                float tooltipHeight = lineHeight * inspectableMembers.Count + 6;
                                var tooltipRect = new Rect(
                                    labelPosition.xMax + 3,
                                    labelPosition.center.y - tooltipHeight / 2,
                                    maxWidth + 16,
                                    tooltipHeight
                                );

                                float textY = tooltipRect.y + 3;
                                foreach (var line in lines)
                                {
                                    GUI.Label(new Rect(tooltipRect.x + 6, textY, tooltipRect.width - 12, lineHeight), line, Styles.prediction);
                                    textY += lineHeight;
                                }
                            }
                        }
                    }
                    else
                    {
                        var label = new GUIContent(valueString, Icons.Type(value?.GetType())?[IconSize.Small]);
                        var labelSize = Styles.prediction.CalcSize(label);
                        var labelPosition = new Rect(position.position - labelSize / 2, labelSize);

                        BeginDim();
                        GUI.Label(labelPosition, label, Styles.prediction);
                        EndDim();
                    }

                    EditorGUIUtility.SetIconSize(previousIconSize);
                }
            }
        }

        protected override void DrawConnection()
        {
            if (connection.destination != null && connection.destination.unit is ValueReroute destinationReroute)
            {
                hideConnection = destinationReroute.hideConnection;

                if (hideConnection)
                {
                    var mousePos = e.mousePosition;

                    if (IsMouseOverConnection(mousePos, sourceHandlePosition.position, destinationHandlePosition.position))
                    {
                        hideConnection = false;
                    }
                    else if (canvas.hoveredWidget != null)
                    {
                        var hoveredItem = canvas.hoveredWidget.item;
                        hideConnection = !(hoveredItem == destinationReroute || hoveredItem == connection.source.unit || hoveredItem == connection.source || hoveredItem == connection.destination);
                    }
                }
            }

            if (!hideConnection)
            {
#if ENABLE_VERTICAL_FLOW
                var color = this.color;

                var sourceWidget = canvas.Widget<IUnitPortWidget>(connection.source);
                var destinationWidget = canvas.Widget<IUnitPortWidget>(connection.destination);

                var highlight = !canvas.isCreatingConnection && (sourceWidget.isMouseOver || destinationWidget.isMouseOver);

                var willDisconnect = sourceWidget.willDisconnect || destinationWidget.willDisconnect;

                if (willDisconnect)
                {
                    color = UnitConnectionStyles.disconnectColor;
                }
                else if (highlight)
                {
                    color = UnitConnectionStyles.highlightColor;
                }
                else if (colorIfActive)
                {
                    if (EditorApplication.isPaused)
                    {
                        if (EditorTimeBinding.frame == ConnectionDebugData.lastInvokeFrame)
                        {
                            color = UnitConnectionStyles.activeColor;
                        }
                    }
                    else
                    {
                        color = Color.Lerp(UnitConnectionStyles.activeColor, color, (EditorTimeBinding.time - ConnectionDebugData.lastInvokeTime) / UnitWidget<IUnit>.Styles.invokeFadeDuration);
                    }
                }

                GraphGUI.DrawConnection(color, sourceHandleEdgeCenter, destinationHandleEdgeCenter, Edge.Right, Edge.Left, null, Vector2.zero, CommunityStyles.relativeBend, CommunityStyles.minBend, CommunityStyles.connectionThickness);
#else
                base.DrawConnection();
#endif
            }
        }

        public override void CachePosition()
        {
            base.CachePosition();

            var rect = new Rect(sourceHandlePosition);

            if (element.destination.unit is ValueReroute desReroute && desReroute.hideConnection)
            {
                rect.width -= 5;
            }
            else if (element.source.connections.Any(c => c.destination.unit is ValueReroute desReroute && desReroute.hideConnection))
            {
                rect.width -= 5;
            }
            else return;

            sourceHandlePosition = rect;
            sourceHandleEdgeCenter = sourceHandlePosition.GetEdgeCenter(Edge.Right);
        }

        private bool IsMouseOverConnection(Vector2 mousePos, Vector2 start, Vector2 end, float threshold = 8f)
        {
            if (!clippingPosition.Contains(mousePos))
            {
                return false;
            }

            float distance = Vector2.Distance(start, end);
            int segments = Mathf.Clamp(Mathf.CeilToInt(distance / 10f), 12, 80);
            float minDist = float.MaxValue;

            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
#if ENABLE_VERTICAL_FLOW
                Vector2 p = GraphGUI.GetPointOnConnection(t, start, end, Edge.Right, Edge.Left, CommunityStyles.relativeBend, CommunityStyles.minBend);
#else
                Vector2 p = GraphGUI.GetPointOnConnection(t, start, end, Edge.Right, Edge.Left, UnitConnectionStyles.relativeBend, UnitConnectionStyles.minBend);
#endif
                float dist = Vector2.Distance(mousePos, p);
                if (dist < minDist) minDist = dist;
                if (minDist < threshold) return true;
            }

            return false;
        }

        public static Color DetermineColor(Type source, Type destination)
        {
            if (destination == typeof(object))
            {
                return DetermineColor(source);
            }

            return DetermineColor(destination);
        }

        public static Color DetermineColor(Type type)
        {
            if (type == null)
            {
                return new Color(0.8f, 0.8f, 0.8f);
            }

            if (type == typeof(string))
            {
                return new Color(1.0f, 0.62f, 0.35f);
            }

            if (type == typeof(bool))
            {
                return new Color(0.86f, 0.55f, 0.92f);
            }

            if (type == typeof(char))
            {
                return new Color(1.0f, 0.90f, 0.40f);
            }

            if (type.IsEnum)
            {
                return new Color(1.0f, 0.63f, 0.66f);
            }

            if (type.IsNumeric())
            {
                return new Color(0.45f, 0.78f, 1f);
            }

            if (type.IsNumericConstruct())
            {
                return new Color(0.45f, 1.00f, 0.82f);
            }

            return new Color(0.60f, 0.88f, 0.00f);
        }

        #endregion


        #region Droplets

        protected override bool showDroplets => BoltFlow.Configuration.animateValueConnections;

        protected override Vector2 GetDropletSize()
        {
            return BoltFlow.Icons.valuePortConnected?[12].Size() ?? 12 * Vector3.one;
        }

        protected override void DrawDroplet(Rect position)
        {
            if (BoltFlow.Icons.valuePortConnected != null && !hideConnection)
            {
                GUI.DrawTexture(position, BoltFlow.Icons.valuePortConnected?[12]);
            }
        }

        #endregion

        private static class Styles
        {
            static Styles()
            {
                prediction = new GUIStyle(EditorStyles.label);
                prediction.normal.textColor = Color.white;
                prediction.fontSize = 9;
                prediction.normal.background = new Color(0, 0, 0, 0.25f).GetPixel();
                prediction.padding = new RectOffset(4, 6, 3, 3);
                prediction.margin = new RectOffset(0, 0, 0, 0);
                prediction.alignment = TextAnchor.MiddleLeft;
                prediction.clipping = TextClipping.Overflow;
            }

            public static readonly GUIStyle prediction;
        }
    }
}
