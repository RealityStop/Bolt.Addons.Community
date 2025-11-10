#if ENABLE_VERTICAL_FLOW
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class ControlInputWidget : UnitInputPortWidget<ControlInput>
    {
        public ControlInputWidget(FlowCanvas canvas, ControlInput port) : base(canvas, port) { }

        protected override Edge edge => Edge.Top;

        protected override Texture handleTextureConnected => PathUtil.Load("ConnectedHandle", CommunityEditorPath.Fundamentals)?[12];

        protected override Texture handleTextureUnconnected => PathUtil.Load("UnconnectedHandle", CommunityEditorPath.Fundamentals)?[12];

        protected override bool colorIfActive => !BoltFlow.Configuration.animateControlConnections || !BoltFlow.Configuration.animateValueConnections;

        public override void CachePosition()
        {
            base.CachePosition();

            var outside = edge.Normal().x;
            var inside = -outside;
            var flip = inside < 0;

            if (showLabel)
            {
                var labelPosition = new Rect(
                    handlePosition.center.x - GetLabelWidth() / 2f,
#if NEW_UNIT_STYLE
                    unitWidget.position.GetEdgeCenter(edge).y - 2,
#else
                    unitWidget.position.GetEdgeCenter(edge).y + 4,
#endif
                    GetLabelWidth(),
                    GetLabelHeight()
                );

                if (flip) labelPosition.x -= labelPosition.width;


                _position = _position.Encompass(labelPosition);
                identifierPosition = identifierPosition.Encompass(labelPosition);
                this.labelPosition = labelPosition;
            }
        }
    }
}
#endif