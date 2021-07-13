using Bolt.Addons.Community.Utility;
using System;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Code
{
    [Serializable]
    public sealed class ClassConstructorDeclaration : ConstructorDeclaration
    {
        public override FlowGraph DefaultGraph()
        {
            return new FlowGraph();
        }
    }

    public enum FunctionType
    {
        Constructor,
        Method,
        Getter,
        Setter
    }
}
