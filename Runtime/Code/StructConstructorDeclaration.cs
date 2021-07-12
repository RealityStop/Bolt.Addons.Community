using Unity.VisualScripting;

namespace Bolt.Addons.Community.Code
{
    public sealed class StructConstructorDeclaration : ConstructorDeclaration
    {
        public override FlowGraph DefaultGraph()
        {
            return new FlowGraph();
        }
    }
}
