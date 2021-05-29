using Bolt.Addons.Community.Utility;
using Unity.VisualScripting;
using System.Collections.Generic;

namespace Bolt.Addons.Community.Fundamentals.Units.logic
{
    public abstract class DelegateUnit : Unit
    {
        [Serialize]
        public IDelegate _delegate;

        [DoNotSerialize]
        public ValueOutput @delegate;

        [DoNotSerialize]
        public List<ValueOutput> parameters = new List<ValueOutput>();

        public GraphReference reference;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput invoke;

        public DelegateUnit() : base() { }

        public DelegateUnit(IDelegate @delegate)
        {
            this._delegate = @delegate;
        }

        protected override void Definition()
        {
            parameters.Clear();

            invoke = ControlOutput("invoke");

            if (_delegate != null)
            {
                @delegate = ValueOutput(_delegate.GetDelegateType(), "delegate", (flow) =>
                {
                    reference = flow.stack.ToReference();
                    InitializeDelegate(flow);
                    return _delegate.GetDelegate();
                });

                for (int i = 0; i < _delegate.parameters.Length; i++)
                {
                    parameters.Add(ValueOutput(_delegate.parameters[i].type, _delegate.parameters[i].name));
                }
            }
        }

        public void AssignParameters(Flow flow, params object[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                flow.SetValue(this.parameters[0], parameters[i]);
            }
        }

        protected abstract void InitializeDelegate(Flow flow);
    }
}
