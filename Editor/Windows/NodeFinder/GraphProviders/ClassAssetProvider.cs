using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Unity.VisualScripting.Community
{
    [GraphProvider]
    public class ClassAssetProvider : BaseGraphProvider
    {
        public ClassAssetProvider(NodeFinderWindow window) : base(window)
        {
        }

        public override string Name => "ClassAssets";

        public override int Order => 4;

        public override IEnumerable<(GraphReference, IGraphElement)> GetElements()
        {
            if (!IsEnabled) yield break;

            var guids = AssetDatabase.FindAssets("t:ClassAsset");
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ClassAsset>(assetPath);
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

        private IEnumerable<GraphReference> GetReferences(ClassAsset asset)
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