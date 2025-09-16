using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [GraphProvider]
    public class StateGraphProvider : BaseGraphProvider
    {
        public StateGraphProvider(NodeFinderWindow window) : base(window)
        {
        }

        public override string Name => "State Graphs";

        public override int Order => 3;

        public override IEnumerable<(GraphReference, IGraphElement)> GetElements()
        {
            if (!IsEnabled) yield break;

            var guids = AssetDatabase.FindAssets("t:StateGraphAsset", null);
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<StateGraphAsset>(assetPath);
                if (asset != null && asset.GetReference()?.graph is not StateGraph) continue;

                var baseRef = asset.GetReference().AsReference();
                foreach (var element in GraphTraversal.TraverseStateGraph<IGraphElement>(baseRef))
                {
                    yield return element;
                }
            }
        }
    }
}