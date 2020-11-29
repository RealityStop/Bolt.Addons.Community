using Bolt.Addons.Community.Utility;
using Ludiq;

namespace Bolt.Addons.Community.Fundamentals.Units.logic
{
    [UnitCategory("Community/Control")]
    [TypeIcon(typeof(Flow))]
    public sealed class FuncUnit : DelegateUnit
    {
        public IFunc _func => _delegate as IFunc;

        [DoNotSerialize]
        public ValueInput @return;

        public FuncUnit() : base() { }

        public FuncUnit(IFunc func) : base(func)
        {
        }

        protected override void InitializeDelegate(Flow flow)
        {
            _func.Initialize(flow, this, () => { flow.Invoke(invoke); return flow.GetValue(@return); });
        }

        protected override void Definition()
        {
            base.Definition();

            if (_func != null)
            {
                @return = ValueInput(_func.ReturnType, "return");
            }
        }
    }
}
