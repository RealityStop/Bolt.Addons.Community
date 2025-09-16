using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.VisualScripting.Community
{
    [GraphProvider]
    public class ScriptMachineProvider : SceneGraphProvider
    {
        public ScriptMachineProvider(NodeFinderWindow window) : base(window)
        {
        }

        public override string Name => "Embed Script Machines";

        public override int Order => 0;

        public override IEnumerable<(GraphReference, IGraphElement)> GetElements()
        {
            foreach (var machine in GetTargetsFromScene<ScriptMachine>().Where(machine => machine.nest.source == GraphSource.Embed))
            {
                if (machine != null && machine.GetReference()?.graph is not FlowGraph) continue;

                var baseRef = machine.GetReference().AsReference();
                foreach (var element in GraphTraversal.TraverseFlowGraph<IGraphElement>(baseRef))
                {
                    yield return element;
                }
            }
        }
    }
}