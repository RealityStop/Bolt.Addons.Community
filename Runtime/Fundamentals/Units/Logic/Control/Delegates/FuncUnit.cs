using Bolt.Addons.Community.Utility;
using System;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Fundamentals
{
    [UnitCategory("Community/Control/Delegates")]
    [TypeIcon(typeof(Flow))]
    public sealed class FuncUnit : DelegateUnit
    {
        public IFunc _func => _delegate as IFunc;

        [DoNotSerialize]
        public ValueInput @return;

        public FuncUnit() : base() { }
        public FuncUnit(IDelegate del) : base(del)
        {

        }

        protected override void InitializeDelegate(Flow flow, bool instance = false)
        {
            _func.Initialize(flow, this, () => { var _flow = Flow.New(flow.stack.AsReference()); _flow.Invoke(invoke); return _flow.GetValue(@return); });
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
