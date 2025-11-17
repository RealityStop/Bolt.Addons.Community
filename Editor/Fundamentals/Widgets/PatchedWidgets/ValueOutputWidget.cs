using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class ValueOutputWidget : UnitOutputPortWidget<ValueOutput>
    {
        public ValueOutputWidget(FlowCanvas canvas, ValueOutput port) : base(canvas, port)
        {
            color = ValueConnectionWidget.DetermineColor(port.type);
        }

        protected override bool showIcon => true;

        protected override bool colorIfActive => !BoltFlow.Configuration.animateControlConnections || !BoltFlow.Configuration.animateValueConnections;

        public override Color color { get; }

        protected override Texture handleTextureConnected
        {
            get
            {
                var connections = port.connections;
                if (connections.Any(c => c.destination.unit is ValueReroute))
                {
                    if (connections.FirstOrDefault(c => c.destination?.unit is ValueReroute)?.destination?.unit is ValueReroute valueReroute && valueReroute.hideConnection)
                    {
                        return PathUtil.Load("PortalConnectionIn", CommunityEditorPath.Fundamentals)?[16];
                    }
                }
                return BoltFlow.Icons.valuePortConnected?[12];
            }
        }

        protected override Texture handleTextureUnconnected => BoltFlow.Icons.valuePortUnconnected?[12];

        protected override void DrawConnectionSource()
        {
            var start = handlePosition.GetEdgeCenter(edge);

            if (window.IsFocused())
            {
                canvas.connectionEnd = mousePosition;
            }

            float minBend = 20f;

            Vector2 size = new Vector2(9, 12);

            var connections = port.connections;
            if (connections.Any(c => c.destination.unit is ValueReroute))
            {
                if (connections.FirstOrDefault(c => c.destination?.unit is ValueReroute)?.destination?.unit is ValueReroute valueReroute && valueReroute.hideConnection)
                {
                    size = new Vector2(16, 16);
                }
            }
            else if (e.alt) size = new Vector2(16, 16);

            GraphGUI.DrawConnection
                (
                    color,
                    start,
                    canvas.connectionEnd,
                    edge,
                    null,
                    e.alt ? PathUtil.Load("PortalConnectionIn", CommunityEditorPath.Fundamentals)?[16] : handleTextureConnected,
                    size,
                    UnitConnectionStyles.relativeBend,
                    minBend
                );
        }


        public override void CachePosition()
        {
            var unitPosition = unitWidget.position;

            var x = unitPosition.GetEdgeCenter(edge).x;
            var outside = edge.Normal().x;
            var inside = -outside;
            var flip = inside < 0;

            var width = VisualScripting.UnitPortWidget<ValueInput>.Styles.handleSize.x;
            var height = VisualScripting.UnitPortWidget<ValueInput>.Styles.handleSize.y;

            bool hide = false;

            var connections = port.connections;
            if (connections.Any(c => c.destination.unit is ValueReroute))
            {
                if (connections.FirstOrDefault(c => c.destination?.unit is ValueReroute)?.destination?.unit is ValueReroute valueReroute && valueReroute.hideConnection)
                {
                    if (valueReroute.hideConnection)
                    {
                        width = 16;
                        height = 16;
                        hide = true;
                    }
                }
            }

            var handleY = y + (EditorGUIUtility.singleLineHeight - Styles.handleSize.y) / 2;
            var handleX = x + (Styles.handleSize.x + Styles.spaceBetweenEdgeAndHandle) * outside;

            var handlePosition = new Rect
                (
                hide ? handleX + 4 : handleX,
                hide ? handleY - 2 : handleY,
                width,
                height
                );

            if (flip)
            {
                handlePosition.x -= handlePosition.width;
            }

            this.handlePosition = handlePosition;

            _position = handlePosition;
            identifierPosition = handlePosition;

            x += Styles.spaceAfterEdge * inside;

            if (showIcon)
            {
                var iconPosition = new Rect
                    (
                    x,
                    y - 1,
                    Styles.iconSize,
                    Styles.iconSize
                    ).PixelPerfect();

                if (flip)
                {
                    iconPosition.x -= iconPosition.width;
                }

                x += iconPosition.width * inside;

                _position = _position.Encompass(iconPosition);
                identifierPosition = identifierPosition.Encompass(iconPosition);

                this.iconPosition = iconPosition;
            }

            if (showIcon && showLabel)
            {
                x += Styles.spaceBetweenIconAndLabel * inside;
            }

            if (showIcon && !showLabel && showInspector)
            {
                x += Styles.spaceBetweenIconAndInspector * inside;
            }

            if (showLabel)
            {
                var labelPosition = new Rect
                    (
                    x,
                    y,
                    GetLabelWidth(),
                    GetLabelHeight()
                    );

                if (flip)
                {
                    labelPosition.x -= labelPosition.width;
                }

                x += labelPosition.width * inside;

                _position = _position.Encompass(labelPosition);
                identifierPosition = identifierPosition.Encompass(labelPosition);

                this.labelPosition = labelPosition;
            }

            if (showLabel && showInspector)
            {
                x += Styles.spaceBetweenLabelAndInspector * inside;
            }

            if (showInspector)
            {
                var inspectorPosition = new Rect
                    (
                    x,
                    y,
                    GetInspectorWidth(),
                    GetInspectorHeight()
                    );

                if (flip)
                {
                    inspectorPosition.x -= inspectorPosition.width;
                }

                x += inspectorPosition.width * inside;

                _position = _position.Encompass(inspectorPosition);

                this.inspectorPosition = inspectorPosition;
            }

            surroundPosition = Styles.surroundPadding.Add(identifierPosition);
        }
    }
}
