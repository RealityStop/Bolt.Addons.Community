#if ENABLE_VERTICAL_FLOW
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public sealed class ControlConnectionWidget : UnitConnectionWidget<ControlConnection>
    {
        public ControlConnectionWidget(FlowCanvas canvas, ControlConnection connection) : base(canvas, connection) { }


        #region Drawing

        public override Color color => Color.white;

        protected override bool colorIfActive => !BoltFlow.Configuration.animateControlConnections || !BoltFlow.Configuration.animateValueConnections;

        #endregion


        #region Droplets

        protected override bool showDroplets => BoltFlow.Configuration.animateControlConnections;

        protected override Vector2 GetDropletSize()
        {
            return BoltFlow.Icons.valuePortConnected?[12].Size() ?? 12 * Vector2.one;
        }

        protected override void DrawDroplet(Rect position)
        {
            if (BoltFlow.Icons.valuePortConnected != null)
            {
                GUI.DrawTexture(position, BoltFlow.Icons.valuePortConnected[12]);
            }
        }

        protected override Edge sourceEdge => Edge.Bottom;

        protected override Edge destinationEdge => Edge.Top;

        #endregion

        public override void CachePosition()
        {
            base.CachePosition();

            sourceHandleEdgeCenter = canvas.Widget<IUnitPortWidget>(connection.source).handlePosition.GetEdgeCenter(Edge.Bottom);
            destinationHandleEdgeCenter = canvas.Widget<IUnitPortWidget>(connection.destination).handlePosition.GetEdgeCenter(Edge.Top);
        }
    }
}
#endif