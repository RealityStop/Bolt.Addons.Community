using UnityEngine;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.CopyToClipboardUnit")]
    [UnitCategory("Community\\Utility")]
    [UnitTitle("Copy To Clipboard")]
    [TypeIcon(typeof(GUIUtility))]
    public class CopyToClipboardUnit : Unit
    {
        [DoNotSerialize]
        [PortKey("enter")]
        public ControlInput enter;

        [DoNotSerialize]
        [PortKey("exit")]
        public ControlOutput exit;

        [DoNotSerialize]
        [PortKey("text")]
        public ValueInput text;

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), CopyTextToClipboard);
            exit = ControlOutput(nameof(exit));
            text = ValueInput<string>(nameof(text), "");

            Succession(enter, exit);
            Requirement(text, enter);
        }

        private ControlOutput CopyTextToClipboard(Flow flow)
        {
            string textToCopy = flow.GetValue<string>(text);
            GUIUtility.systemCopyBuffer = textToCopy;
            return exit;
        }
    }
}
