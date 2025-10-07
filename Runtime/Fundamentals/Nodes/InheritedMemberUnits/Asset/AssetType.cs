using System;

namespace Unity.VisualScripting.Community
{
    [TypeIcon(typeof(Type))]
    public class AssetType : AssetMemberUnit
    {
        [Obsolete(Serialization.ConstructorWarning)]
        public AssetType() { }
        public AssetType(CodeAsset asset)
        {
            this.asset = asset;
        }

        public CodeAsset asset;

        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput type;

        protected override void Definition()
        {
            type = ValueOutput<object>(nameof(type), (flow) => asset.title);
        }
    }

}