using System;
using Ludiq;
using Bolt;

namespace Lasm.BoltAddons.Helpers
{
    
    [UnitCategory("Tools/Graphs")]
    public class GetCurrentGraph : Unit
    {
        [DoNotSerialize]
        ValueOutput thisGraph;

        protected override void Definition()
        {
            Func<Recursion, IGraph> currentGraph = getCurrentGraph => ReturnCurrentGraph();
            thisGraph = ValueOutput<IGraph>("graph", currentGraph);
        }

        private IGraph ReturnCurrentGraph()
        {
            return base.graph;
        }
    }
}
