namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.ComparisonBranch")]
    public abstract class ComparisonBranch : Unit, IBranchUnit
    {
        public ComparisonBranch() : base() { }

        /// <summary>
        /// The entry point for the branch.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter { get; private set; }

        /// <summary>
        /// The first input.
        /// </summary>
        [DoNotSerialize]
        public ValueInput a { get; protected set; }

        /// <summary>
        /// The second input.
        /// </summary>
        [DoNotSerialize]
        public ValueInput b { get; protected set; }


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


        protected override void Definition()
        {
            enter = ControlInput("enter", Enter);
            
            ifTrue = ControlOutput("ifTrue");
            ifFalse = ControlOutput("ifFalse");

            Succession(enter, ifTrue);
            Succession(enter, ifFalse);
            Requirement(a, enter);
            Requirement(b, enter);
        }

        public abstract bool Comparison(Flow flow);

        public ControlOutput Enter(Flow flow)
        {
            if (Comparison(flow))
                return ifTrue;
            else
                return ifFalse;
        }
    }
}