using Bolt;
using Ludiq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Bolt.Addons.Community.Fundamentals.Units.Utility.Editor
{
    [Widget(typeof(ValueReroute))]
    public sealed class ValueRerouteWidget : UnitWidget<ValueReroute>
    {
        private static ValueReroute addedUnit = null;
        private static bool keyPressed;
        private static bool keyUp;
        private int connections;
        private int frames = 0;

        public ValueRerouteWidget(FlowCanvas canvas, ValueReroute unit) : base(canvas, unit)
        {
        }

        public override void DrawForeground()
        {
            GraphGUI.Node(new Rect(position.x, position.y, _position.width, _position.height), NodeShape.Square, NodeColor.Gray, isSelected);
        }

        private Type lastType = typeof(object);

        protected override bool showIcons => true;

        public override void Update()
        {
                if (unit.input != null && unit.input.connection != null && unit.input.connection.sourceExists)
                {
                    unit.portType = unit.input.connection.source.type;
                }
                else
                {
                    unit.portType = typeof(object);
                }

                if (lastType != null && unit.portType != null && lastType != unit.portType)
                {
                    lastType = unit.portType;
                    unit.Define();
                }

                if (addedUnit == null && keyPressed && SourceIsReroute())
                {
                    addedUnit = new ValueReroute();
                    addedUnit.position = canvas.mousePosition - new Vector2(14, 14);
                    ((FlowGraph)graph).units.Add(addedUnit);
                    unit.output.ValidlyConnectTo(addedUnit.input);
                    canvas.connectionSource = addedUnit.output;
                }
                else
                {
                    if (addedUnit == null && HasSource() && HasConnections() && IsConnecting() && SourceIsReroute() && DestinationIsNotReroute())
                    {
                        if (canvas.hoveredWidget as UnitInputPortWidget<ValueInput> != null && !keyPressed) canvas.CancelConnection();
                    }
                }

                if (addedUnit != null) canvas.connectionSource = addedUnit.output;

                if (!keyPressed)
                {
                    addedUnit = null;
                }

            if (frames < 10) frames++; ;
        }

        private bool HasSource()
        {
            return canvas.connectionSource != null;
        }

        public override void HandleCapture()
        {
            base.HandleCapture();
            keyPressed = e.keyCode == KeyCode.Space;
            keyUp = e.IsKeyUp(KeyCode.Space);
        }

        private bool DestinationIsNotReroute()
        {
            return canvas.connectionSource.connections.Any((connection) => { return connection.destination.unit.GetType() != typeof(ValueReroute); });
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
    }
}