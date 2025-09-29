namespace Unity.VisualScripting.Community
{
    [FuzzyOption(typeof(AssetType))]
    internal sealed class AssetTypeOption : UnitOption<AssetType>
    {
        public ClassAsset classAsset;
        public AssetTypeOption(AssetType unit) : base(unit)
        {
            classAsset = unit.asset;
        }

        public override IUnit InstantiateUnit()
        {
            return new AssetType(unit.asset);
        }
    }
    
}