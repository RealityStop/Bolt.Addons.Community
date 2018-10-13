using Bolt;
using Bolt.Community.Addons.Utility;
using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Addons.Community.Fundamentals.Units.FlowEvents
{
    [UnitCategory("Events/Editor")]
    [UnitShortTitle("Manual Event")]
    [UnitTitle("Manual Event")]
    public class ManualEvent : ManualEventUnit<EmptyEventArgs>
    {
        protected override string hookName { get { return "EditorButtonEvent"; } }

        [UnitHeaderInspectable]
        [UnitButton("TriggerButton")]
        public UnitButton triggerButton;

        public void TriggerButton(GraphReference reference)
        {
            Flow flow = Flow.New(reference);
            flow.Invoke(trigger);
        }
    }
}
