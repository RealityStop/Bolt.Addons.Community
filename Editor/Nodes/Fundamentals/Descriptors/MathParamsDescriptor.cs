namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(MathParamNode))]
    public class MathParamsDescriptor : UnitDescriptor<MathParamNode>
    {
        public MathParamsDescriptor(MathParamNode unit) : base(unit) { }

        protected override EditorTexture DefinedIcon()
        {
            switch (unit.OperationType)
            {
                case MathParamNode.MathType.Add: return typeof(Unity.VisualScripting.GenericSum).Icon();
                case MathParamNode.MathType.Subtract: return typeof(Unity.VisualScripting.GenericSubtract).Icon();
                case MathParamNode.MathType.Multiply: return typeof(Unity.VisualScripting.GenericMultiply).Icon();
                case MathParamNode.MathType.Divide: return typeof(Unity.VisualScripting.GenericDivide).Icon();
                default: return base.DefinedIcon();
            }
        }
    }
}