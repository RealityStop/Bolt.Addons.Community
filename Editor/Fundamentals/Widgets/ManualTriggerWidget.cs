using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Widget(typeof(ManualEvent))]
    public class ManualEventWidget : UnitWidget<ManualEvent>
    {
        public ManualEventWidget(FlowCanvas canvas, ManualEvent unit) : base(canvas, unit)
        {

        }
        protected override NodeColorMix color => NodeColor.Green;

        public override bool foregroundRequiresInput => true;
        public override void DrawForeground()
        {
            base.DrawForeground();

            var buttonPosition = new Rect(position.x + 1, position.y + 40 + 5, position.width - 8 + 6, 24);

            if (GUI.Button(buttonPosition, "Trigger"))
            {
                unit.TriggerButton(GraphWindow.activeReference);
            }
        }


    }
}