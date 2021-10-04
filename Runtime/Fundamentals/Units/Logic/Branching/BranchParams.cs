using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [UnitShortTitle("Branch")]
    [UnitTitle("Branch (Params)")]
    [UnitCategory("Community\\Control")]
    [TypeIcon(typeof(Unity.VisualScripting.If))]
    [RenamedFrom("Bolt.Addons.Community.Logic.Units.BranchAnd")]
    [RenamedFrom("Bolt.Addons.Community.Logic.Units.BranchParams")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.BranchParams")]
    public sealed class BranchParams : LogicParamNode, IBranchUnit
    {
        public BranchParams() { }

        [PortLabelHidden]
        [DoNotSerialize]
        public ControlInput enter { get; private set; }

        [PortLabel("True")]
        [DoNotSerialize]
        public ControlOutput exitTrue { get; private set; }

        [PortLabel("False")]
        [DoNotSerialize]
        public ControlOutput exitFalse { get; private set; }

        [PortLabel("Next")]
        [DoNotSerialize]
        public ControlOutput exitNext { get; private set; }

        [Serialize]
        [Inspectable]
        [InspectorLabel("Next Output")]
        private bool showNext = default(bool);


        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Branch);
            exitTrue = ControlOutput(nameof(exitTrue));
            exitFalse = ControlOutput(nameof(exitFalse));
            if (showNext)
            {
                exitNext = ControlOutput(nameof(exitNext));
                Succession(enter, exitNext);
            }

            base.Definition();

            Succession(enter, exitTrue);
            Succession(enter, exitFalse);
        }

        protected override void BuildRelations(ValueInput arg)
        {
            Requirement(arg, enter);
        }

        private bool GetValue(Flow flow)
        {
            switch (BranchingType)
            {
                case BranchType.And:
                    foreach (var item in arguments)
                    {
                        if (!flow.GetValue<bool>(item))
                            return false;
                    }
                    return true;
                case BranchType.Or:
                    foreach (var item in arguments)
                    {
                        if (flow.GetValue<bool>(item))
                            return true;
                    }
                    return false;
                case BranchType.GreaterThan:
                    {
                        bool NumericComparison(float a, float b, bool allowEquals)
                        {
                            return (a > b) || (allowEquals && Mathf.Approximately(a, b));
                        }

                        return NumericComparison(flow.GetValue<float>(arguments[0]), flow.GetValue<float>(arguments[1]), AllowEquals);
                    }
                case BranchType.LessThan:
                    {
                        bool NumericComparison(float a, float b, bool allowEquals)
                        {
                            return (a < b) || (allowEquals && Mathf.Approximately(a, b));
                        }

                        return NumericComparison(flow.GetValue<float>(arguments[0]), flow.GetValue<float>(arguments[1]), AllowEquals);
                    }
                case BranchType.Equal:
                    if (Numeric)
                    {
                        var target = flow.GetValue<float>(arguments[0]);

                        for (int i = 1; i < arguments.Count; i++)
                        {
                            if (!Mathf.Approximately(target, flow.GetValue<float>(arguments[i])))
                                return false;
                        }
                        return true;
                    }
                    else
                    {
                        var target = flow.GetValue<object>(arguments[0]);

                        for (int i = 1; i < arguments.Count; i++)
                        {
                            if (!OperatorUtility.Equal(target, flow.GetValue<object>(arguments[i])))
                                return false;
                        }
                        return true;
                    }
                default:
                    return false;
            }
        }


        private ControlOutput Branch(Flow flow)
        {
            if (showNext)
            {
                if (GetValue(flow))
                    flow.Invoke(exitTrue);
                else
                    flow.Invoke(exitFalse);

                return exitNext;
            }
            else
                return GetValue(flow) ? exitTrue : exitFalse;
        }
    }
}