using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
#if VISUAL_SCRIPTING_1_7
using SMachine = Unity.VisualScripting.ScriptMachine;
#else
using SMachine = Unity.VisualScripting.FlowMachine;
#endif
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
            foreach (var machine in GetTargetsFromScene<SMachine>().Where(machine => machine.nest.source == GraphSource.Embed))
            {
                if (machine != null && !(machine.GetReference()?.graph is FlowGraph)) continue;

                var baseRef = machine.GetReference().AsReference();
                foreach (var element in GraphTraversal.TraverseFlowGraph<IGraphElement>(baseRef))
                {
                    yield return element;
                }
            }
        }
    }
}