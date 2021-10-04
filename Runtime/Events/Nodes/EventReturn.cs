#if VISUAL_SCRIPTING_1_7
using SMachine = Unity.VisualScripting.ScriptMachine;
#else
using SMachine = Unity.VisualScripting.FlowMachine;
#endif

namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// The Unit that returns your flow and value back to the Complete port of the initial trigger unit.
    /// </summary>
    [UnitCategory("Events/Community/Returns")][UnitShortTitle("Return")][UnitSubtitle("Return Event")]
    [RenamedFrom("Lasm.BoltExtensions.EventReturn")]
    [RenamedFrom("Lasm.UAlive.EventReturn")]
    [RenamedFrom("Bolt.Addons.Community.ReturnEvents.EventReturn")]
    public sealed class EventReturn : Unit
    {
        /// <summary>
        /// The Control Input entered when we want to return back.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter;

        /// <summary>
        /// The Value Input that set the value to return back.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput value;

        /// <summary>
        /// The data output from the ReturnEventUnit. This is necessary for the unit to return back to the original unit, as it stores the data about which unit we came from.
        /// </summary>
        [DoNotSerialize]
        public ValueInput data;

        private GraphReference triggerReference;

        private SMachine machine;

        /// <summary>
        /// Defines the ports of the unit.
        /// </summary>
        protected override void Definition()
        {
            enter = ControlInput("enter", Enter);
            data = ValueInput<ReturnEventData>("data");
            value = ValueInput<object>("value");
        }

        /// <summary>
        /// Set the data to return back, then triggers a return back to the ReturnEventUnit.
        /// </summary>
        public ControlOutput Enter(Flow flow)
        {
            var _data = flow.GetValue<ReturnEventData>(data);
            var val = flow.GetValue(value);
            if (_data.args.trigger != null) _data.args.trigger.storingValue = val;
            if (_data.args.isCallback)
            {
                _data.args.callback?.DynamicInvoke(val);
            }
            else
            {
                TriggerReturnEvent.Trigger(_data.args);
            }
            return null;
        }
    }
}