using System;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [UnitCategory("Community\\Documentation")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.Units.Documenting.Todo")]
    public class Todo : Unit
    {

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter { get; private set; }


        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput exit { get; private set; }

        [Serialize]
        [Inspectable]
        [InspectorLabel("Error on hit")]
        public bool ErrorIfHit { get; set; } = false;


        [Serialize]
        [Inspectable]
        [UnitHeaderInspectable]
        [InspectorLabel("Message")]
        [InspectorTextArea()]
        public string CustomMessage { get; set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), (x) => Enter(x));
            exit = ControlOutput(nameof(exit));

            Succession(enter, exit);
        }

        private ControlOutput Enter(Flow x)
        {
            string message = string.Format("Todo{0}", string.IsNullOrEmpty(CustomMessage) ? "" : " - " + CustomMessage);

            Debug.LogWarning(message);

            if (ErrorIfHit)
                throw new NotImplementedException(message);

            return exit;
        }
    }
}