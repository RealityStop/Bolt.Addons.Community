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
        [DoNotSerialize]
        public ValueOutput bound;

        object combined;

        public BindDelegateUnit() : base() { }
        public BindDelegateUnit(IDelegate @delegate)
        {
            this._delegate = @delegate;
        }

        protected override void Definition()
        {
            enter = ControlInput("enter", (flow) =>
            {
                Delegate _a = flow.GetValue(a) as Delegate;
                Delegate _b = flow.GetValue(b) as Delegate;
                combined = Delegate.Combine(_a, _b);
                return exit;
            });

            exit = ControlOutput("exit");

            a = ValueInput(_delegate.GetDelegateType(), "a");
            b = ValueInput(_delegate.GetDelegateType(), "b");
            bound = ValueOutput(_delegate.GetDelegateType(), "delegate", (flow) => { return combined; });
        }
    }
}
