using System.Collections.Generic;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.LogicParamNode")]
    public abstract class LogicParamNode : VariadicNode<bool>
    {
        public enum BranchType { And, Or, GreaterThan, LessThan, Equal }


        [SerializeAs(nameof(BranchingType))]
        private BranchType _branchingType;

        [DoNotSerialize]
        [Inspectable, UnitHeaderInspectable("Condition")]
        public BranchType BranchingType { get { return _branchingType; } set { _branchingType = value; } }



        [DoNotSerialize]
        public bool supportsEqual => BranchingType == BranchType.LessThan || BranchingType == BranchType.GreaterThan;




        /// <summary>
        /// Adds Equals to the comparison such that a Less comparison becomes a Less Than or Equals and a Greater comparison becomes a Greater Than or Equals.  Has no effect on other comparison modes.
        /// </summary>
        [Serialize]
        [InspectorLabel("[[<,>]] Allow Equals")]
        [Inspectable]
        [InspectorExpandTooltip]
        public bool AllowEquals = false;



        [DoNotSerialize]
        public bool supportsNumeric => BranchingType == BranchType.Equal;

        /// <summary>
        /// Makes an Equals comparison a Numeric comparison, allowing approximately close values to match.
        /// </summary>
        [Serialize]
        [Inspectable]
        [InspectorLabel("[[ = ]] Numeric")]
       [InspectableIf(nameof(supportsNumeric))]
        public bool Numeric = false;


        protected override void Definition()
        {
            arguments = new List<ValueInput>();

            switch (BranchingType)
            {
                case BranchType.And:
                case BranchType.Or:
                    ConstructArgs<bool>();
                    break;
                case BranchType.GreaterThan:
                case BranchType.LessThan:
                    ConstructArgs<float>();
                    break;
                case BranchType.Equal:
                    if (Numeric)
                        ConstructArgs<float>();
                    else
                        ConstructArgs<object>();
                    break;
                default:
                    break;
            }
        }


        protected override int ArgumentLimit()
        {
            if (BranchingType == BranchType.GreaterThan || BranchingType == BranchType.LessThan)
                return 2;
            return base.ArgumentLimit();
        }
    }
}