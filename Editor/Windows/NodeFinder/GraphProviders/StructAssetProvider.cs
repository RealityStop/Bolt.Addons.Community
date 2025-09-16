using System.Collections.Generic;
using UnityEditor;

namespace Unity.VisualScripting.Community
{
    [GraphProvider]
    public class StructAssetProvider : BaseGraphProvider
    {
        public StructAssetProvider(NodeFinderWindow window) : base(window)
        {
        }

        public override string Name => "StructAssets";

        public override int Order => 5;

        public override IEnumerable<(GraphReference, IGraphElement)> GetElements()
        {
            if (!IsEnabled) yield break;

            var guids = AssetDatabase.FindAssets("t:StructAsset");
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<StructAsset>(assetPath);
                if (asset == null) continue;

                foreach (var reference in GetReferences(asset))
                {
                    foreach (var element in GraphTraversal.TraverseFlowGraph<IGraphElement>(reference))
                    {
                        yield return element;
                    }
                }
            }
        }

        private IEnumerable<GraphReference> GetReferences(StructAsset asset)
        {
            foreach (var constructor in asset.constructors)
            {
                yield return constructor.GetReference().AsReference();
            }
            foreach (var variable in asset.variables)
            {
                if (variable.isProperty)
                {
                    if (variable.get)
                        yield return variable.getter.GetReference().AsReference();
                    if (variable.set)
                        yield return variable.setter.GetReference().AsReference();
                }
            }
            foreach (var method in asset.methods)
            {
                yield return method.GetReference().AsReference();
            }
        }
    }
}