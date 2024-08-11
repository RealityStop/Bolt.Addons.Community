namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(AssetType))]
    public class AssetTypeDescriptor : UnitDescriptor<AssetType>
    {
        public AssetTypeDescriptor(AssetType target) : base(target)
        {
        }
    
        protected override string DefinedSubtitle()
        {
            if(target.asset == null)
            {
                return "Type";
            }
            return target.asset.title;
        }
    }
    
}