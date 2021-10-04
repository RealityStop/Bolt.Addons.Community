using System.Collections.Generic;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.DelegateUnit")]
    public abstract class DelegateNode : Unit
    {
        [Serialize]
        public IDelegate _delegate;

        [DoNotSerialize]
        public ValueOutput @delegate;

        public string name;

        [DoNotSerialize]
        public List<ValueOutput> parameters = new List<ValueOutput>();

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput invoke;

        [UnitHeaderInspectable]
        private bool variable;
        private object[] values;

        public DelegateNode() : base() { }

        public DelegateNode(IDelegate @delegate)
        {
            this._delegate = @delegate;
        }

        protected override void Definition()
        {
            isControlRoot = true;

            parameters.Clear();

            invoke = ControlOutput("invoke");

            if (_delegate != null)
            {
                @delegate = ValueOutput(_delegate.GetType(), "delegate", (flow) =>
                {
                    var _flow = Flow.New(flow.stack.AsReference());
                    InitializeDelegate(_flow, _delegate.initialized);
                    return _delegate;
                });

                for (int i = 0; i < _delegate.parameters.Length; i++)
                {
                    var index = i;
                    parameters.Add(ValueOutput(_delegate.parameters[i].type, _delegate.parameters[i].name, (flow) =>
                    {
                        return values[index];
                    }));
                }
            }
        }

        public override void Instantiate(GraphReference instance)
        {
            base.Instantiate(instance);
            if (_delegate != null) _delegate.initialized = false;
        }

        public void AssignParameters(Flow flow, params object[] parameters)
        {
            values = parameters;
        }

        protected abstract void InitializeDelegate(Flow flow, bool instance = false);
    }
}
