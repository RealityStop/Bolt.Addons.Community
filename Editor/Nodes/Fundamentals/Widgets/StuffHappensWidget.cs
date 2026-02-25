using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Widget(typeof(StuffHappens))]
    public sealed class StuffHappensWidget : UnitWidget<StuffHappens>
    {

        public StuffHappensWidget(FlowCanvas canvas, StuffHappens unit) : base(canvas, unit)
        {
        }

        protected override NodeColorMix baseColor
        {
            get
            {
                return new NodeColorMix() { red = 0.6578709f, green = 1f };
            }
        }

#if NEW_UNIT_STYLE
        protected override bool isSpecialPortsColor => true;

        protected override Color? PortsbackgroundColor => new Color(1f, 0.32f, 0.1f);
#endif
    }
}