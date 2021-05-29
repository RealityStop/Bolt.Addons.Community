using Bolt.Addons.Community.Utility;
using Ludiq;
using System;

namespace Bolt.Addons.Community.Fundamentals.Units.logic
{
    [UnitCategory("Community/Control")]
    public sealed class BindDelegateUnit : Unit
    {
        [Serialize]
        public IDelegate _delegate;
        [DoNotSerialize]
        public ControlInput enter;
        [DoNotSerialize]
        public ControlOutput exit;
        [DoNotSerialize]
        public ValueInput a;
        [DoNotSerialize]
        public ValueInput b;

        public BindDelegateUnit() : base() { }
        public BindDelegateUnit(IDelegate @delegate)
        {
            this._delegate = @delegate;
        }

        protected override void Definition()
        {
            enter = ControlInput("enter", (flow) =>
            {
                if (_delegate is IAction)
                {
                    ((IAction)_delegate).Bind(flow.GetValue(a), flow.GetValue(b));
                }

                if (_delegate is IFunc)
                {
                    ((IFunc)_delegate).Bind(flow.GetValue(a), flow.GetValue(b));
                }
                return exit;
            });

            exit = ControlOutput("exit");

            a = ValueInput(_delegate.GetDelegateType(), "a");
            b = ValueInput(_delegate.GetDelegateType(), "b");
        }
    }
}
