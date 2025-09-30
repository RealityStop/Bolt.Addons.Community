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
                case LogicParamNode.BranchType.And: return typeof(Unity.VisualScripting.And).Icon();
                case LogicParamNode.BranchType.Or: return typeof(Unity.VisualScripting.Or).Icon();
                case LogicParamNode.BranchType.GreaterThan: return typeof(Unity.VisualScripting.Greater).Icon();
                case LogicParamNode.BranchType.LessThan: return typeof(Unity.VisualScripting.Less).Icon();
                case LogicParamNode.BranchType.Equal: return typeof(Unity.VisualScripting.Equal).Icon();
                default: return base.DefinedIcon();
            }
        }
    }
}