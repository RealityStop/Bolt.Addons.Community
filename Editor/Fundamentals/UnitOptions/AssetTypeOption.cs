namespace Unity.VisualScripting.Community
{
    [FuzzyOption(typeof(AssetType))]
    internal sealed class AssetTypeOption : UnitOption<AssetType>
    {
        public CodeAsset codeAsset;
        public AssetTypeOption(AssetType unit) : base(unit)
        {
            codeAsset = unit.asset;
        }

        public override IUnit InstantiateUnit()
        {
            return new AssetType(unit.asset);
        }
    }
    
}