using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [GraphProvider]
    public class StateMachineProvider : SceneGraphProvider
    {
        public StateMachineProvider(NodeFinderWindow window) : base(window)
        {
        }

        public override string Name => "Embed State Machines";

        public override int Order => 1;

        public override IEnumerable<(GraphReference, IGraphElement)> GetElements()
        {
            if (!IsEnabled) yield break;

            foreach (var machine in GetTargetsFromScene<StateMachine>().Where(machine => machine.nest.source == GraphSource.Embed))
            {
                if (machine != null && machine.GetReference()?.graph is null) continue;

                var baseRef = machine.GetReference().AsReference();
                foreach (var element in GraphTraversal.TraverseStateGraph<IGraphElement>(baseRef))
                {
                    yield return element;
                }
            }
        }
    }
}