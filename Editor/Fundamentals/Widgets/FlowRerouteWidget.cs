using Bolt;
using Ludiq;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Bolt.Addons.Community.Fundamentals.Units.Utility
{
    [Widget(typeof(FlowReroute))]
    public sealed class FlowRerouteWidget : UnitWidget<FlowReroute>
    {
        private static FlowReroute addedUnit = null;
        private static bool keyPressed;

        public FlowRerouteWidget(FlowCanvas canvas, FlowReroute unit) : base(canvas, unit)
        {
        }

        public override void DrawForeground()
        {
            Ludiq.GraphGUI.Node(new Rect(position.x, position.y, _position.width, _position.height), NodeShape.Square, NodeColor.Gray, isSelected);
        }

        protected override bool showIcons => true;
        
        public override bool foregroundRequiresInput => true;

        public override void Update()
        {
            if (addedUnit == null && keyPressed && SourceIsReroute())
            {
                addedUnit = new FlowReroute();
                addedUnit.position = canvas.mousePosition - new Vector2(14, 14);
                ((FlowGraph)graph).units.Add(addedUnit);
                unit.output.ValidlyConnectTo(addedUnit.input);
                canvas.connectionSource = addedUnit.output;
            }
            else
            {
                if (addedUnit == null && HasSource() && HasConnections() && IsConnecting() && SourceIsReroute() && DestinationIsNotReroute())
                {
                    if (canvas.hoveredWidget as UnitInputPortWidget<ControlInput> != null && !keyPressed) canvas.CancelConnection();
                }
            }

            if (addedUnit != null) canvas.connectionSource = addedUnit.output;

            if (!keyPressed)
            {
                addedUnit = null;
            }
        }

        private bool DestinationIsNotReroute()
        {
            return canvas.connectionSource.connections.Any((connection) => { return connection.destination.unit.GetType() != typeof(FlowReroute); });
        }

        private bool HasSource()
        {
            return canvas.connectionSource != null;
        }

        private bool HasConnections()
        {
            return canvas.connectionSource.connections != null;
        }

        private bool SourceIsReroute()
        {
            return canvas.connectionSource == unit.output;
        }

        private bool IsConnecting()
        {
            return canvas.isCreatingConnection;
        }

        public override void CachePosition()
        {
            _position.x = unit.position.x;
            _position.y = unit.position.y;
            _position.width = 26;
            _position.height = 26;

            inputs[0].y = _position.y + 5;
            outputs[0].y = _position.y + 5;
        }

        public override void HandleCapture()
        {
            base.HandleCapture();
            keyPressed = e.keyCode == KeyCode.Space;
        }
    }
} 