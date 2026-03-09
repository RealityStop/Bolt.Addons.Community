namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(LogicParams))]
    public class LogicParamsDescriptor : UnitDescriptor<LogicParams>
    {
        public LogicParamsDescriptor(LogicParams unit) : base(unit) { }

        protected override EditorTexture DefinedIcon()
        {
            switch (unit.BranchingType)
            {
                case LogicParamNode.BranchType.And: return typeof(And).Icon();
                case LogicParamNode.BranchType.Or: return typeof(Or).Icon();
                case LogicParamNode.BranchType.GreaterThan: return unit.AllowEquals ? typeof(GreaterOrEqual).Icon() : typeof(Greater).Icon();
                case LogicParamNode.BranchType.LessThan: return unit.AllowEquals ? typeof(LessOrEqual).Icon() : typeof(Less).Icon();
                case LogicParamNode.BranchType.Equal: return typeof(Equal).Icon();
                default: return base.DefinedIcon();
            }
        }
    }
}