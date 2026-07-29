using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Widget(typeof(ManualEvent))]
    public class ManualEventWidget : UnitWidget<ManualEvent>
    {
        public ManualEventWidget(FlowCanvas canvas, ManualEvent unit) : base(canvas, unit)
        {

        }
        protected override NodeColorMix baseColor => NodeColor.Green;
#if !ENABLE_VERTICAL_FLOW
        public override bool foregroundRequiresInput => true;
        public override void DrawForeground()
        {
            base.DrawForeground();
#if NEW_UNIT_STYLE
            const int yPadding = 48;
#else
            const int yPadding = 45;
#endif
            var buttonPosition = new Rect(position.x + 1, position.y + yPadding, position.width - 8 + 6, 24);

            if (GUI.Button(buttonPosition, "Trigger"))
            {
                unit.TriggerButton(GraphWindow.activeReference);
            }
        }
#endif
    }
}