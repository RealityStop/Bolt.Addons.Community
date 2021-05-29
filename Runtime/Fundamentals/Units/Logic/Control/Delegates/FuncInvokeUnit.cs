using Bolt.Addons.Community.Utility;
using Unity.VisualScripting;
using System.Collections.Generic;

namespace Bolt.Addons.Community.Fundamentals.Units.logic
{
    [UnitCategory("Community/Control")]
    [TypeIcon(typeof(Flow))]
    public class FuncInvokeUnit : DelegateInvokeUnit
    {
        public IFunc _func => _delegate as IFunc;

        [DoNotSerialize]
        public ValueOutput @return;

        public FuncInvokeUnit() : base() { }
        public FuncInvokeUnit(IFunc @func) : base(@func) { }

        protected override bool isPure => true;

        protected override void Definition()
        {
            base.Definition();

            if (_func != null)
            {
                @return = ValueOutput(_func.ReturnType, "return", (flow) =>
                {
                    var values = new List<object>();
                    var act = flow.GetValue<System.Delegate>(@delegate);

                    for (int i = 0; i < parameters.Count; i++)
                    {
                        values.Add(flow.GetValue(parameters[i]));
                    }

                    return act.DynamicInvoke(values.ToArray());
                });
            }
        }
    }
}
