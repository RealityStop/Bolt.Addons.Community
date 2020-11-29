using Bolt.Addons.Community.Utility;
using Ludiq;
using System.Collections.Generic;
using UnityEngine;

namespace Bolt.Addons.Community.Fundamentals.Units.logic
{
    [UnitCategory("Community/Control")]
    [TypeIcon(typeof(Flow))]
    public class ActionInvokeUnit : Unit
    {
        [Serialize]
        public IAction _action;

        [DoNotSerialize]
        public ValueInput action;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput exit;

        public List<ValueInput> parameters = new List<ValueInput>();

        public ActionInvokeUnit() : base() { }

        public ActionInvokeUnit(IAction action)
        {
            _action = action;
        }

        protected override void Definition()
        {
            parameters.Clear();

            enter = ControlInput("enter", (flow)=> 
            {
                var values = new List<object>();
                var act = flow.GetValue<System.Delegate>(action);

                for (int i = 0; i < parameters.Count; i++)
                {
                    values.Add(flow.GetValue(parameters[i]));
                }

                act.DynamicInvoke(values.ToArray());
                return exit;
            });

            exit = ControlOutput("exit");

            if (_action != null)
            {
                action = ValueInput(_action.GetActionType(), "action");

                for (int i = 0; i < _action.parameters.Length; i++)
                {
                    parameters.Add(ValueInput(_action.parameters[i].type, _action.parameters[i].name));
                }
            }
        }
    }
}
