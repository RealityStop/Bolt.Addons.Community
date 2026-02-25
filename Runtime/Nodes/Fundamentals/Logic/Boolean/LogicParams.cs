namespace Unity.VisualScripting.Community
{
    [UnitShortTitle("Logic")]
    [UnitTitle("Logic (Params)")]
    [UnitCategory("Community\\Logic")]
    [RenamedFrom("Bolt.Addons.Community.Logic.Units.LogicParams")]
    [RenamedFrom("Bolt.Addons.Community.Logic.Units.OrParam")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.LogicParams")]
    public sealed class LogicParams : LogicParamNode
    {
        public LogicParams() { }

        [PortLabel("Result")]
        [DoNotSerialize]
        public ValueOutput output { get; private set; }

        protected override void Definition()
        {
            output = ValueOutput<bool>(nameof(output), GetValue);

            base.Definition();
        }

        protected override void BuildRelations(ValueInput arg)
        {
            Requirement(arg, output);
        }

        private bool GetValue(Flow flow)
        {
            foreach (var item in arguments)
            {
                switch (BranchingType)
                {
                    case BranchType.And:
                        if (!flow.GetValue<bool>(item))
                            return false;
                        break;
                    case BranchType.Or:
                        if (flow.GetValue<bool>(item))
                            return true;
                        break;
                    default:
                        return false;
                }
            }

            if (BranchingType == BranchType.And)
                return true;

            if (BranchingType == BranchType.Or)
                return false;

            return false;
        }
    }
}