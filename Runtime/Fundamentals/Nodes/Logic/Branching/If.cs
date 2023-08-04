using UnityEngine;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Fundamentals
{
    [UnitTitle("If")]
    [UnitCategory("Community/Control")]
    [TypeIcon(typeof(Unity.VisualScripting.If))]
    public class If : Unit
    {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput In;

        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput Condition;

        [DoNotSerialize]
        public ControlOutput True;
        [DoNotSerialize]
        public ControlOutput False;
        [DoNotSerialize]
        public ControlOutput Finished;

        protected override void Definition()
        {
            In = ControlInput(nameof(In), Enter);
            True = ControlOutput(nameof(True));
            False = ControlOutput(nameof(False));
            Finished = ControlOutput(nameof(Finished));

            Condition = ValueInput<bool>(nameof(Condition));

            Succession(In, True);
            Succession(In, False);
            Succession(In, Finished);
        }

        private ControlOutput Enter(Flow flow) 
        {
            bool condition = flow.GetValue<bool>(Condition);

            if (flow.isCoroutine)
            {
                GraphReference reference = flow.stack.ToReference();
                Flow _flow = Flow.New(reference);
                _flow.StartCoroutine(Finished);
            }
            else
            {
                flow.Invoke(Finished);
            }

            if (condition)
            {
                return True;
            }
            else
            {
                return False;
            }
            
        }
    }  
}
