namespace Unity.VisualScripting.Community
{
    public class ScriptGraphConstructorDeclaration : ConstructorDeclaration
    {
        public override FlowGraph DefaultGraph()
        {
            return new FlowGraph();
        }
    }
}