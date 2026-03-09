using System;

namespace Unity.VisualScripting.Community.Deprecated
{
    /// <summary>
    /// Branches flow by checking if a condition is true or false.
    /// </summary>
    [UnitCategory("Community\\Control")]
    [RenamedFrom("Bolt.Addons.Community.Variables.Units.BranchNext")]
    [RenamedFrom("Bolt.Addons.Community.Logic.Units.BranchNext")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.BranchNext")]
    [UnitOrder(0)]
    [Obsolete("Use the Branch (Param) node instead!  It supersede this one!")]
    public sealed class BranchNext : Unit, IBranchUnit
    {
        public BranchNext() : base() { }

        /// <summary>
        /// The entry point for the branch.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter { get; private set; }

        /// <summary>
        /// The condition to check.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput condition { get; private set; }

        /// <summary>
        /// The action to execute if the condition is true.
        /// </summary>
        [DoNotSerialize]
        [PortLabel("True")]
        public ControlOutput ifTrue { get; private set; }

        /// <summary>
        /// The action to execute if the condition is false.
        /// </summary>
        [DoNotSerialize]
        [PortLabel("False")]
        public ControlOutput ifFalse { get; private set; }


        /// <summary>
        /// The action to execute if the condition is false.
        /// </summary>
        [DoNotSerialize]
        [PortLabel("Next")]
        public ControlOutput onNext { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput("enter", Enter);
            condition = ValueInput<bool>("condition");
            ifTrue = ControlOutput("ifTrue");
            ifFalse = ControlOutput("ifFalse");
            onNext = ControlOutput("onNext");

            Succession(enter, ifTrue);
            Succession(enter, ifFalse);
            Succession(enter, onNext);
            Requirement(condition, enter);
        }

        public ControlOutput Enter(Flow flow)
        {
            if (flow.GetValue<bool>(condition))
            {
                flow.Invoke(ifTrue);
            }
            else
            {
                flow.Invoke(ifFalse);
            }
            return onNext;
        }
    }
}