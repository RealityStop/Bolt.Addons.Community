namespace Unity.VisualScripting.Community
{
    
    [Descriptor(typeof(InheritedMethodCall))]
    public class InheritedMethodCallDescriptor : UnitDescriptor<InheritedMethodCall>
    {
        public InheritedMethodCallDescriptor(InheritedMethodCall target) : base(target)
        {
        }
    
        protected override EditorTexture DefinedIcon()
        {
            return target.member.type.Icon();
        }
    
        protected override string DefinedTitle()
        {
            return "this." + target.member.name;
        }
    
        protected override string DefinedShortTitle()
        {
            return target.member.name;
        }
    }
}