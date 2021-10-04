using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Widget(typeof(ValueReroute))]
    public sealed class ValueRerouteWidget : UnitWidget<ValueReroute>
    {
        public ValueRerouteWidget(FlowCanvas canvas, ValueReroute unit) : base(canvas, unit)
        {
        }

        public override void DrawForeground()
        {
            var inputHasConnection = inputs[0].port.hasAnyConnection;
            var outputHasConnection = outputs[0].port.hasAnyConnection;
            mouseIsOver = new Rect(_position.x - 20, _position.y - 10, mouseIsOver ? 80 : 40, 40).Contains(mousePosition);

#if VISUAL_SCRIPTING_1_7_3
            _position.width = 26;
            GraphGUI.Node(new Rect(position.x, position.y + 3, 26, _position.height - 4), NodeShape.Square, NodeColor.Gray, isSelected);
#else
            if (isSelected || mouseIsOver || !inputHasConnection || !outputHasConnection)
            {
                _position.width = 26;
                GraphGUI.Node(new Rect(position.x, position.y + 3, 26, _position.height-4), NodeShape.Square, NodeColor.Gray, isSelected);
            }
            else
            {
                _position.width = -19;
            }
#endif

            Reposition();
        }

        EditorTexture valueIcon;

        public override bool foregroundRequiresInput => true;

        private bool mouseIsOver;

        public override void CachePosition()
        {
            var inputPort = inputs[0].port;
            var outputPort = outputs[0].port;
            var inputHasConnection = inputPort.hasAnyConnection;
            var outputHasConnection = outputPort.hasAnyConnection;
            _position.x = unit.position.x;
            _position.y = unit.position.y;

#if VISUAL_SCRIPTING_1_7_3
            _position.width = 26;
#else
            _position.width = !inputHasConnection || !outputHasConnection || isSelected || mouseIsOver ? 26 : - 19;
#endif

            _position.height = 20;

            inputs[0].y = _position.y + 5;
            outputs[0].y = _position.y + 5;

            if (valueIcon == null && (inputPort.Descriptor()).description.icon != null) valueIcon = ((UnitPortDescriptor)inputPort.Descriptor()).description.icon;

#if !VISUAL_SCRIPTING_1_7_3
            if (inputHasConnection && !outputHasConnection) { ((UnitPortDescriptor)inputPort.Descriptor()).description.icon = null; }
            ((UnitPortDescriptor)inputPort.Descriptor()).description.icon = !inputHasConnection || isSelected || mouseIsOver ? valueIcon : null;
            ((UnitPortDescriptor)outputPort.Descriptor()).description.icon = !outputHasConnection || isSelected || mouseIsOver ? valueIcon : null;
#endif
        }
    }
}