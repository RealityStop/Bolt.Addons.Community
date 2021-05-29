using Bolt.Addons.Community.Utility;
using Unity.VisualScripting;
using System.Collections.Generic;

namespace Bolt.Addons.Community.Fundamentals.Units.logic
{
    [UnitCategory("Community/Control")]
    [TypeIcon(typeof(Flow))]
    public abstract class DelegateInvokeUnit : Unit
    {
        [Serialize]
        public IDelegate _delegate;

        [DoNotSerialize]
        public ValueInput @delegate;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput exit;

        public List<ValueInput> parameters = new List<ValueInput>();

        protected virtual bool isPure => false;

        public DelegateInvokeUnit() : base() { }

        public DelegateInvokeUnit(IDelegate @delegate)
        {
            _delegate = @delegate;
        }

        protected override void Definition()
        {
            parameters.Clear();

            if (!isPure)
            {
                enter = ControlInput("enter", (flow) =>
                {
                    var values = new List<object>();
                    var act = flow.GetValue<System.Delegate>(@delegate);

                    for (int i = 0; i < parameters.Count; i++)
                    {
                        values.Add(flow.GetValue(parameters[i]));
                    }

                    act.DynamicInvoke(values.ToArray());
                    return exit;
                });

                exit = ControlOutput("exit");
            }

            if (_delegate != null)
            {
                @delegate = ValueInput(_delegate.GetDelegateType(), "delegate");

                for (int i = 0; i < _delegate.parameters.Length; i++)
                {
                    parameters.Add(ValueInput(_delegate.parameters[i].type, _delegate.parameters[i].name));
                }
            }
        }
    }
}
