namespace Unity.VisualScripting.Community
{
    [Widget(typeof(LogNode))]
    public sealed class LogNodeWidget : UnitWidget<LogNode>
    {
        public LogNodeWidget(FlowCanvas canvas, LogNode unit) : base(canvas, unit)
        {
        }

        protected override NodeColorMix baseColor
        {
            get
            {
                switch (unit.type)
                {
                    case Unity.VisualScripting.Community.LogType.Log: return NodeColor.Gray;
                    case Unity.VisualScripting.Community.LogType.Warning: return NodeColor.Orange;
                    case Unity.VisualScripting.Community.LogType.Error: return NodeColor.Red;
                    default: return NodeColor.Gray;
                }
            }
        }
    }
}
