using System;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community
{
    [Widget(typeof(TriggerCustomEvent))]
    public class TriggerCustomEventWidget : UnitWidget<TriggerCustomEvent>
    {
        public TriggerCustomEventWidget(FlowCanvas canvas, TriggerCustomEvent unit) : base(canvas, unit)
        {
        }

        protected override IEnumerable<DropdownOption> contextOptions
        {
            get
            {
                foreach (var option in base.contextOptions)
                {
                    yield return option;
                }

                if (!unit.name.hasValidConnection && !Flow.CanPredict(unit.name, reference))
                    yield break;

                yield return new DropdownOption((Action)FindAll, "Find/All");
                yield return new DropdownOption((Action)FindTriggers, "Find/Triggers");
                yield return new DropdownOption((Action)FindEvents, "Find/Events");
            }
        }

        private void FindAll()
        {
            var name = Flow.Predict<string>(unit.name, reference);
            NodeFinderWindow.Open($"{name} [TriggerCustomEvent] | {name} [CustomEvent]");
        }

        private void FindTriggers()
        {
            var name = Flow.Predict<string>(unit.name, reference);
            NodeFinderWindow.Open($"{name} [TriggerCustomEvent]");
        }

        private void FindEvents()
        {
            var name = Flow.Predict<string>(unit.name, reference);
            NodeFinderWindow.Open($"{name} [CustomEvent]");
        }
    }
}