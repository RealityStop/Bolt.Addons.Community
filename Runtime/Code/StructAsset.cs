using System.Linq;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [CreateAssetMenu(menuName = "Visual Scripting/Community/Code/Struct")]
    [RenamedFrom("Bolt.Addons.Community.Code.StructAsset")]
    public class StructAsset : MemberTypeAsset<StructFieldDeclaration, StructMethodDeclaration, StructConstructorDeclaration>, IGraphRoot
    {
        public IGraph childGraph => GetFirstGraph();

        public bool isSerializationRoot => true;

        public Object serializedObject => this;

        public IGraph DefaultGraph()
        {
            return new FlowGraph();
        }

        public GraphPointer GetReference()
        {
            return GetFirstReference();
        }


        private GraphReference GetFirstReference()
        {
            var variables = this.variables.Where(variable => variable.isProperty);
            if (constructors.Count > 0 && ((constructors[0].GetReference() as GraphReference).graph as FlowGraph).units.Count > 1)
            {
                return constructors[0].GetReference() as GraphReference;
            }
            else if (variables.Count() > 0)
            {
                var first = variables.First();
                if (first.get && ((first.getter.GetReference() as GraphReference).graph as FlowGraph).units.Count > 1)
                {
                    return first.getter.GetReference() as GraphReference;
                }
                else if (first.set && ((first.setter.GetReference() as GraphReference).graph as FlowGraph).units.Count > 1)
                {
                    return first.setter.GetReference() as GraphReference;
                }
            }
            else if (methods.Count > 0 && ((methods[0].GetReference() as GraphReference).graph as FlowGraph).units.Count > 1)
            {
                return methods[0].GetReference() as GraphReference;
            }
            return null;
        }

        private IGraph GetFirstGraph()
        {
            var variables = this.variables.Where(variable => variable.isProperty);
            if (constructors.Count > 0 && (constructors[0]?.graph).units.Count > 1)
            {
                return constructors[0].graph;
            }
            else if (variables.Count() > 0)
            {
                var first = variables.First();
                if (first.get && first.getter.graph.units.Count > 1)
                {
                    return first.getter.graph;
                }
                else if (first.set && first.setter.graph.units.Count > 1)
                {
                    return first.setter.graph;
                }
            }
            else if (methods.Count > 0 && methods[0].graph.units.Count > 1)
            {
                return methods[0].graph;
            }
            return null;
        }
    }
}
