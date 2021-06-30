using Bolt.Addons.Community.Utility;
using Unity.VisualScripting;
using System.Collections.Generic;
using System;

namespace Bolt.Addons.Community.Fundamentals
{
    public abstract class DelegateUnit : Unit, IGraphElementWithData
    {
        public IGraphElementData CreateData()
        {
            return new Data();
        }

        public sealed class Data : IGraphElementData
        {
            public object[] values;
        }

        [Serialize]
        public IDelegate _delegate;

        [DoNotSerialize]
        public ValueOutput @delegate;

        public string name;

        [DoNotSerialize]
        public List<ValueOutput> parameters = new List<ValueOutput>();

        public GraphReference reference;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput invoke;

        public bool initialized;

        [UnitHeaderInspectable]
        private bool variable;

        public DelegateUnit() : base() { }

        public DelegateUnit(IDelegate @delegate)
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
                    reference = flow.stack.ToReference();
                    if (!_delegate.initialized) { var _flow = Flow.New(reference); InitializeDelegate(_flow); }
                    return _delegate;
                });

                for (int i = 0; i < _delegate.parameters.Length; i++)
                {
                    var index = i;
                    parameters.Add(ValueOutput(_delegate.parameters[i].type, _delegate.parameters[i].name, (flow)=> { return flow.stack.GetElementData<Data>(this).values[index]; }));
                }
            }
        }
         
        public void AssignParameters(Flow flow, params object[] parameters)
        {
            flow.stack.GetElementData<Data>(this).values = parameters;
        }

        protected abstract void InitializeDelegate(Flow flow);
    }
}
