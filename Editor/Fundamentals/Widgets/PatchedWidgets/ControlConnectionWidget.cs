using System.Reflection;
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

        protected override void DrawConnection()
        {
            var color = this.color;

            var sourceWidget = canvas.Widget<IUnitPortWidget>(connection.source);
            var destinationWidget = canvas.Widget<IUnitPortWidget>(connection.destination);

            var highlight = !canvas.isCreatingConnection && (sourceWidget.isMouseOver || destinationWidget.isMouseOver);

            var willDisconnect = sourceWidget.willDisconnect || destinationWidget.willDisconnect;

            if (willDisconnect)
            {
                color = UnitConnectionStyles.disconnectColor;
            }
            else if (highlight)
            {
                color = UnitConnectionStyles.highlightColor;
            }
            else if (colorIfActive)
            {
                if (EditorApplication.isPaused)
                {
                    if (EditorTimeBinding.frame == ConnectionDebugData.lastInvokeFrame)
                    {
                        color = UnitConnectionStyles.activeColor;
                    }
                }
                else
                {
                    color = Color.Lerp(UnitConnectionStyles.activeColor, color, (EditorTimeBinding.time - ConnectionDebugData.lastInvokeTime) / UnitWidget<IUnit>.Styles.invokeFadeDuration);
                }
            }

            float minBend = 20f;

            var thickness = 3;

            GraphGUI.DrawConnection(
                color,
                sourceHandleEdgeCenter,
                destinationHandleEdgeCenter,
                Edge.Bottom,
                Edge.Top,
                null,
                Vector2.zero,
                UnitConnectionStyles.relativeBend,
                minBend,
                thickness
            );
        }

        #endregion

        public override void CachePosition()
        {
            base.CachePosition();

            sourceHandleEdgeCenter = canvas.Widget<IUnitPortWidget>(connection.source).handlePosition.GetEdgeCenter(Edge.Bottom);
            destinationHandleEdgeCenter = canvas.Widget<IUnitPortWidget>(connection.destination).handlePosition.GetEdgeCenter(Edge.Top);
        }
    }
}
