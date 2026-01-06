#if ENABLE_VERTICAL_FLOW
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Widget(typeof(FlowReroute))]
    public sealed class FlowRerouteWidget : UnitWidget<FlowReroute>
    {
        public FlowRerouteWidget(FlowCanvas canvas, FlowReroute unit) : base(canvas, unit)
        {
        }

        public override void DrawForeground()
        {
            var inputHasConnection = inputs[0].port.hasAnyConnection;
            var outputHasConnection = outputs[0].port.hasAnyConnection;
            mouseIsOver = new Rect(_position.x, _position.y - 10, 26, mouseIsOver ? 80 : 40).Contains(mousePosition);

#if VISUAL_SCRIPTING_1_7_3
            _position.height = 20;
            GraphGUI.Node(new Rect(position.x, position.y + 3, 26, _position.height - 4), NodeShape.Square, color, isSelected);
            GUI.DrawTexture(new Rect(_position.center.x - 8, _position.y + 5, 16, 16), typeof(Flow).Icon()?[IconSize.Small]);
#else
            if (isSelected || mouseIsOver || !inputHasConnection || !outputHasConnection)
            {
                _position.height = 20;
                GraphGUI.Node(new Rect(position.x, position.y + 3, 26, _position.height - 4), NodeShape.Square, color, isSelected);
                GUI.DrawTexture(new Rect(_position.center.x - 8, _position.y + 5, 16, 16), typeof(Flow).Icon()?[IconSize.Small]);
            }
            else
            {
                _position.height = -19;
            }
#endif
            Reposition();
        }

        EditorTexture flowIcon;

        protected override bool snapToGrid => unit.SnapToGrid;

        public override bool foregroundRequiresInput => true;

        private bool mouseIsOver;

        public override void CachePosition()
        {
            var inputPort = inputs[0].port;
            var outputPort = outputs[0].port;
            var inputHasConnection = inputPort.hasAnyConnection;
            var outputHasConnection = outputPort.hasAnyConnection;
            base.CachePosition();
            _position.x = unit.position.x;
            _position.y = unit.position.y;


#if VISUAL_SCRIPTING_1_7_3
            _position.height = 20;
#else
            _position.height = !inputHasConnection || !outputHasConnection || isSelected || mouseIsOver ? 20 : -19;
#endif

            _position.width = 26;

            inputs[0].y = !inputHasConnection || !outputHasConnection || isSelected || mouseIsOver ? _position.yMin - 22 : _position.y + 5;
            outputs[0].y = !inputHasConnection || !outputHasConnection || isSelected || mouseIsOver ? _position.yMax + 5 : _position.y + 5;
            (inputs[0] as ControlInputWidget).x = _position.center.x - 6;
            (outputs[0] as ControlOutputWidget).x = _position.center.x - 6;

#if !VISUAL_SCRIPTING_1_7_3
            if (flowIcon == null && (inputPort.Descriptor()).description.icon != null) flowIcon = ((UnitPortDescriptor)inputPort.Descriptor()).description.icon;

            if (inputHasConnection && !outputHasConnection) { ((UnitPortDescriptor)inputPort.Descriptor()).description.icon = null; }
            ((UnitPortDescriptor)inputPort.Descriptor()).description.icon = !inputHasConnection || isSelected || mouseIsOver ? flowIcon : null;
            ((UnitPortDescriptor)outputPort.Descriptor()).description.icon = !outputHasConnection || isSelected || mouseIsOver ? flowIcon : null;
#endif
        }
    }
}

#else

using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Widget(typeof(FlowReroute))]
    public sealed class FlowRerouteWidget : UnitWidget<FlowReroute>
    {
        public FlowRerouteWidget(FlowCanvas canvas, FlowReroute unit) : base(canvas, unit)
        {
        }

        public override void DrawForeground()
        {
            var inputHasConnection = inputs[0].port.hasAnyConnection;
            var outputHasConnection = outputs[0].port.hasAnyConnection;
            mouseIsOver = new Rect(_position.x - 20, _position.y - 10, mouseIsOver ? 80 : 40, 40).Contains(mousePosition);

#if VISUAL_SCRIPTING_1_7_3
            _position.width = 26;
            GraphGUI.Node(new Rect(position.x, position.y + 3, 26, _position.height-4), NodeShape.Square, color, isSelected);
#else
            if (isSelected || mouseIsOver || !inputHasConnection || !outputHasConnection)
            {
                _position.width = 26;
                GraphGUI.Node(new Rect(position.x, position.y + 3, 26, _position.height - 4), NodeShape.Square, color, isSelected);
            }
            else
            {
                _position.width = -19;
            }
#endif
            Reposition();

        }

        EditorTexture flowIcon;

        protected override bool snapToGrid => unit.SnapToGrid;

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
            _position.width = !inputHasConnection || !outputHasConnection || isSelected || mouseIsOver ? 26 : -19;
#endif

            _position.height = 20;

            inputs[0].y = _position.y + 5;
            outputs[0].y = _position.y + 5;

#if !VISUAL_SCRIPTING_1_7_3
            if (flowIcon == null && (inputPort.Descriptor()).description.icon != null) flowIcon = ((UnitPortDescriptor)inputPort.Descriptor()).description.icon;

            if (inputHasConnection && !outputHasConnection) { ((UnitPortDescriptor)inputPort.Descriptor()).description.icon = null; }
            ((UnitPortDescriptor)inputPort.Descriptor()).description.icon = !inputHasConnection || isSelected || mouseIsOver ? flowIcon : null;
            ((UnitPortDescriptor)outputPort.Descriptor()).description.icon = !outputHasConnection || isSelected || mouseIsOver ? flowIcon : null;
#endif
        }
    }
}
#endif