using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(AssetType))]
    public class AssetTypeGenerator : NodeGenerator<AssetType>
    {
        public AssetTypeGenerator(Unit unit) : base(unit)
        {
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.Write("typeof".ConstructHighlight());
            writer.Write("(");
            writer.Write(Unit.asset.title.TypeHighlight());
            writer.Write(")");
        }
    }
}