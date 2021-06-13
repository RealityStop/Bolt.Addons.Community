using Bolt.Addons.Community.Utility;
using Unity.VisualScripting;
using System.Collections.Generic;
using System;

namespace Bolt.Addons.Community.Fundamentals
{
    public abstract class DelegateUnit : Unit
    {
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

        protected override void AfterDefine()
        {
            base.AfterDefine();

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
                    reference = flow.stack.root.GetReference() as GraphReference;
                    if (!_delegate.initialized) { InitializeDelegate(Flow.New(reference)); }
                    return _delegate;
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
                flow.SetValue(this.parameters[i], parameters[i]);
            }
        }

        protected abstract void InitializeDelegate(Flow flow);
    }
}
