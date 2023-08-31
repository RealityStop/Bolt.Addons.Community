using Unity.VisualScripting.Community.Utility;
using System;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    public sealed class UnitConstructorDeclaration : ConstructorDeclaration
    {
        public override FlowGraph DefaultGraph()
        {
            return new FlowGraph();
        }
    }
}
