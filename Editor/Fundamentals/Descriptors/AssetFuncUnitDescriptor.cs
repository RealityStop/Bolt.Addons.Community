namespace Unity.VisualScripting.Community 
{
    [Descriptor(typeof(AssetFuncUnit))]
    public class AssetFuncUnitDescriptor : UnitDescriptor<AssetFuncUnit>
    {
        public AssetFuncUnitDescriptor(AssetFuncUnit target) : base(target)
        {
        }
    
        protected override string DefinedSurtitle()
        {
            return target.method.parentAsset.title;
        }
    
        protected override EditorTexture DefinedIcon()
        {
            return target.method.returnType.Icon();
        }
    
        protected override string DefinedTitle()
        {
            return target.method.parentAsset.title + "." + target.method.methodName;
        }
    
        protected override string DefinedShortTitle()
        {
            return target.method.methodName;
        }
    } 
}
