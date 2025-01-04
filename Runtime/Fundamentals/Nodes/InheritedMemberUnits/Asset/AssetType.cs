using System;

namespace Unity.VisualScripting.Community
{
    [TypeIcon(typeof(Type))]
    public class AssetType : AssetMemberUnit
    {
        [Obsolete(Serialization.ConstructorWarning)]
        public AssetType() { }
        public AssetType(ClassAsset asset)
        {
            this.asset = asset;
        }

        public ClassAsset asset;

        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput type;

        protected override void Definition()
        {
            type = ValueOutput<object>(nameof(type), (flow) => asset.title);
        }
    }

}