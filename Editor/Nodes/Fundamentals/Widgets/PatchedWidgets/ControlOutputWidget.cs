#if ENABLE_VERTICAL_FLOW
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class ControlOutputWidget : UnitOutputPortWidget<ControlOutput>
    {
        public ControlOutputWidget(FlowCanvas canvas, ControlOutput port) : base(canvas, port) { }

        protected override Texture handleTextureConnected => CommunityStyles.controlPortConnected;

        protected override Texture handleTextureUnconnected => CommunityStyles.controlPortUnconnected;

        protected override Edge edge => Edge.Bottom;

        protected override Edge connectionEndEdge => Edge.Top;

        protected override bool colorIfActive => !BoltFlow.Configuration.animateControlConnections || !BoltFlow.Configuration.animateValueConnections;
    }
}
#endif