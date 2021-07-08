using System.Collections.Generic;
using Unity.VisualScripting;
using Bolt.Addons.Libraries.Humility;
using System;

namespace Bolt.Addons.Community.Processing
{
    public sealed class GraphProcessor : GlobalProcess
    {
        private Type[] types;
        private List<GraphProcess> processes = new List<GraphProcess>();

        public override void Process()
        {
            GraphReference reference = null;
            if (CanProcess(out reference))
            {
                for (int i = 0; i < processes.Count; i++)
                {
                    if (reference.graph.GetType() == processes[i].graphType)
                    {
                        processes[i].Process(reference.graph, GraphWindow.activeContext.canvas);
                    }
                }
            }
        }

        private bool CanProcess(out GraphReference reference)
        {
            reference = GraphWindow.activeReference;
            var active = GraphWindow.active;
            return reference != null && active != null && active.hasFocus;
        }

        public override void OnBind()
        {
            GraphReference reference = null;
            if (CanProcess(out reference))
            {
                for (int i = 0; i < processes.Count; i++)
                {
                    if (reference.graph.GetType() == processes[i].graphType)
                    {
                        processes[i].OnBind(reference.graph as FlowGraph, GraphWindow.activeContext.canvas as FlowCanvas);
                    }
                }
            }
        }

        public override void OnUnbind()
        {
            GraphReference reference = null;
            if (CanProcess(out reference))
            {
                for (int i = 0; i < processes.Count; i++)
                {
                    if (reference.graph.GetType() == processes[i].graphType)
                    {
                        processes[i].OnUnbind(reference.graph as FlowGraph, GraphWindow.activeContext.canvas as FlowCanvas);
                    }
                }
            }
        }

        public override void OnInitialize()
        {
            types = typeof(GraphProcess).Get().Derived(false);

            for (int i = 0; i < types.Length; i++)
            {
                processes.Add(types[i].New() as GraphProcess);
            }
        }
    }
}