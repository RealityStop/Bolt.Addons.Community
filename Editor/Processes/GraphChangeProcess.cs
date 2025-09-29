using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class GraphChangeProcess : GraphProcess<FlowGraph, FlowCanvas>
    {
        public List<FlowGraph> subscribedGraphs = new List<FlowGraph>();
        public override void Process(FlowGraph graph, FlowCanvas canvas)
        {
            if (graph != null && !subscribedGraphs.Contains(graph))
            {
                subscribedGraphs.Add(graph);
                graph.elements.ItemAdded += OnGraphChanged;
            }
        }

        private static void OnGraphChanged(IGraphElement element)
        {
        }
    }
}
