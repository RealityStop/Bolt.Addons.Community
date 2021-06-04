using Bolt.Addons.Community.Utility;
using Unity.VisualScripting;
using System;

namespace Bolt.Addons.Community.Fundamentals.Units.logic
{
    public abstract class BindDelegateUnit<TDelegateInterface> : Unit where TDelegateInterface : IDelegate
    {
        [Serialize]
        public TDelegateInterface _delegate;
        [DoNotSerialize]
        public ControlInput enter;
        [DoNotSerialize]
        public ControlOutput exit;
        [DoNotSerialize]
        public ValueInput a;
        [DoNotSerialize]
        public ValueInput b;

        public BindDelegateUnit() : base() { }

        public BindDelegateUnit(TDelegateInterface @delegate)
        {
            this._delegate = @delegate;
        }

        protected override void Definition()
        {
            enter = ControlInput("enter", (flow) =>
            {
                if (_delegate is IAction)
                {
                    flow.GetValue<IAction>(a).Bind(flow.GetValue<TDelegateInterface>(b));
                }
                else
                {
                    if (_delegate is IFunc)
                    {
                        flow.GetValue<IFunc>(a).Bind(flow.GetValue<TDelegateInterface>(b));
                    }
                }
                return exit;
            });

            exit = ControlOutput("exit");

            if (_delegate != null)
            {
                a = ValueInput(_delegate.GetType(), "a");
                b = ValueInput(_delegate.GetType(), "b");
            }
            else
            {
                a = ValueInput(typeof(object), "a");
                b = ValueInput(typeof(object), "b");
            }

            Requirement(a, enter);
            Requirement(b, enter);
            Succession(enter, exit);
        }
    }
}
