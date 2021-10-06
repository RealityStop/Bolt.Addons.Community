using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.Humility;
using System;

namespace Unity.VisualScripting.Community
{
    public sealed class GraphProcessor : GlobalProcess
    {
        private Type[] types;
        private List<GraphProcess> processes = new List<GraphProcess>();
        private bool isBound;

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
            for (int i = 0; i < processes.Count; i++)
            {
                processes[i].Bind();
            }
        }

        public override void OnUnbind()
        {
            for (int i = 0; i < processes.Count; i++)
            {
                processes[i].Unbind();
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