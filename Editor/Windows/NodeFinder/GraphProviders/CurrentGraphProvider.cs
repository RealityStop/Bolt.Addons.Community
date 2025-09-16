using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [GraphProvider]
    public class CurrentGraphProvider : BaseGraphProvider
    {
        public CurrentGraphProvider(NodeFinderWindow window) : base(window) { }

        public override string Name => "Current Graph";

        public override int Order => 0;

        // private GraphReference GetRootReference(GraphReference reference)
        // {
        //     if (reference == null) return null;
        //     while (reference.isChild) reference = reference.ParentReference(false);
        //     return reference;
        // }

        public override IEnumerable<(GraphReference, IGraphElement)> GetElements()
        {
            if (!IsEnabled) yield break;

            // var activeGraph = GetRootReference(GraphWindow.activeReference);
            var activeGraph = GraphWindow.activeReference;
            if (activeGraph == null) yield break;

            if (activeGraph.graph is FlowGraph)
            {
                foreach (var element in GraphTraversal.TraverseFlowGraph<IGraphElement>(activeGraph))
                    yield return element;
            }
            else if (activeGraph.graph is StateGraph)
            {
                foreach (var element in GraphTraversal.TraverseStateGraph<IGraphElement>(activeGraph))
                    yield return element;
            }
        }
    }
}
