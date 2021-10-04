namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(BranchParams))]
    public class BranchParamsDescriptor : UnitDescriptor<BranchParams>
    {
        public BranchParamsDescriptor(BranchParams unit) : base(unit) { }

        protected override EditorTexture DefinedIcon()
        {
            switch (unit.BranchingType)
            {
                case LogicParamNode.BranchType.And:
                    return PathUtil.Load("Branch_And", CommunityEditorPath.Fundamentals);
                case LogicParamNode.BranchType.Or:
                    return PathUtil.Load("Branch_Or", CommunityEditorPath.Fundamentals);
                case LogicParamNode.BranchType.GreaterThan:
                    if (unit.AllowEquals)
                        return PathUtil.Load("Branch_Greater_Equal", CommunityEditorPath.Fundamentals);
                    else
                        return PathUtil.Load("Branch_Greater", CommunityEditorPath.Fundamentals);
                case LogicParamNode.BranchType.LessThan:
                    if (unit.AllowEquals)
                        return PathUtil.Load("Branch_Less_Equal", CommunityEditorPath.Fundamentals);
                    else
                        return PathUtil.Load("Branch_Less", CommunityEditorPath.Fundamentals);
                case LogicParamNode.BranchType.Equal:
                    return PathUtil.Load("Branch_Equal", CommunityEditorPath.Fundamentals);
                default:
                    return PathUtil.Load("arrow_switch", CommunityEditorPath.Fundamentals);
            }

        }

        protected override void DefinedPort(IUnitPort port, UnitPortDescription description)
        {
            base.DefinedPort(port, description);

            if (port == unit.exitNext)
                description.summary = "Exit control flow after True/False evaluation.  Will always be called following the comparison.";

            if (port == unit.exitTrue)
            {
                switch (unit.BranchingType)
                {
                    case LogicParamNode.BranchType.And:
                        description.label = "True";
                        description.summary = "Exit control flow if all inputs are true.";
                        break;
                    case LogicParamNode.BranchType.Or:
                        description.label = "True";
                        description.summary = "Exit control flow if any of the inputs are true.";
                        break;
                    case LogicParamNode.BranchType.GreaterThan:
                        description.label = "Greater";
                        description.summary = string.Format("Exit control flow if the first input is greater than {0}the second.", unit.AllowEquals ? "or equal to " : "");
                        break;
                    case LogicParamNode.BranchType.LessThan:
                        description.label = "Less";
                        description.summary = string.Format("Exit control flow if the first input is less than {0}the second.", unit.AllowEquals ? "or equal to " : "");
                        break;
                    case LogicParamNode.BranchType.Equal:
                        description.label = "Equal";
                        description.summary = "Exit control flow if all of the inputs are equal.";
                        break;
                    default:
                        break;
                }
            }

            if (port == unit.exitFalse)
            {
                switch (unit.BranchingType)
                {
                    case LogicParamNode.BranchType.And:
                        description.label = "False";
                        description.summary = "Exit control flow if one of the inputs is false.";
                        break;
                    case LogicParamNode.BranchType.Or:
                        description.label = "False";
                        description.summary = "Exit control flow if none of the inputs are true.";
                        break;
                    case LogicParamNode.BranchType.GreaterThan:
                        description.label = "Less";
                        description.summary = string.Format("Exit control flow if the first input is less than {0}the second.", unit.AllowEquals ? "" : "or equal to ");
                        break;
                    case LogicParamNode.BranchType.LessThan:
                        description.label = "Greater";
                        description.summary = string.Format("Exit control flow if the first input is greater than {0}the second.", unit.AllowEquals ? "" : "or equal to ");
                        break;
                    case LogicParamNode.BranchType.Equal:
                        description.label = "Not Equal";
                        description.summary = "Exit control flow if any of the inputs are not equal.";
                        break;
                    default:
                        break;
                }
            }
        }
    }
}