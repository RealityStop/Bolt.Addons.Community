#if ENABLE_VERTICAL_FLOW
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class ControlOutputWidget : UnitOutputPortWidget<ControlOutput>
    {
        public ControlOutputWidget(FlowCanvas canvas, ControlOutput port) : base(canvas, port) { }

        protected override Texture handleTextureConnected => PathUtil.Load("ConnectedHandle", CommunityEditorPath.Fundamentals)?[12];

        protected override Texture handleTextureUnconnected => PathUtil.Load("UnconnectedHandle", CommunityEditorPath.Fundamentals)?[12];

        protected override Edge edge => Edge.Bottom;

        protected override bool colorIfActive => !BoltFlow.Configuration.animateControlConnections || !BoltFlow.Configuration.animateValueConnections;

        // public override void CachePosition()
        // {
        //     base.CachePosition();
        //     var unitPosition = unitWidget.position;

        //     var y = unitPosition.yMax + Styles.spaceBetweenEdgeAndHandle * 2;

        //     handlePosition = new Rect(x, y, Styles.handleSize.x, Styles.handleSize.y);
        //     _position = handlePosition;
        //     identifierPosition = handlePosition;
        //     surroundPosition = Styles.surroundPadding.Add(identifierPosition);
        // }
    }
}
#endif