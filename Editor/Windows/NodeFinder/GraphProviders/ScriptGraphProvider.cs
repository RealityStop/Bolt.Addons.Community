using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [GraphProvider]
    public class ScriptGraphProvider : BaseGraphProvider
    {
        public ScriptGraphProvider(NodeFinderWindow window) : base(window)
        {
        }

        public override string Name => "Script Graphs";

        public override int Order => 2;

        public override IEnumerable<(GraphReference, IGraphElement)> GetElements()
        {
            if (!IsEnabled) yield break;

            var guids = AssetDatabase.FindAssets("t:ScriptGraphAsset", null);
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ScriptGraphAsset>(assetPath);
                if (asset != null && !(asset.GetReference()?.graph is FlowGraph)) continue;

                var baseRef = asset.GetReference().AsReference();
                foreach (var element in GraphTraversal.TraverseFlowGraph<IGraphElement>(baseRef))
                {
                    yield return element;
                }
            }
        }

    }
}