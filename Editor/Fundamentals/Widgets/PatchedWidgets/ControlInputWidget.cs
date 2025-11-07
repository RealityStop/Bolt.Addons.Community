using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class ControlInputWidget : UnitInputPortWidget<ControlInput>
    {
        public ControlInputWidget(FlowCanvas canvas, ControlInput port) : base(canvas, port) { }

        protected override Edge edge => Edge.Top;

        protected override bool showLabel => false;

        protected override Texture handleTextureConnected => PathUtil.Load("ConnectedHandle", CommunityEditorPath.Fundamentals)?[12];

        protected override Texture handleTextureUnconnected => PathUtil.Load("UnconnectedHandle", CommunityEditorPath.Fundamentals)?[12];

        protected override bool colorIfActive => !BoltFlow.Configuration.animateControlConnections || !BoltFlow.Configuration.animateValueConnections;
    }
}