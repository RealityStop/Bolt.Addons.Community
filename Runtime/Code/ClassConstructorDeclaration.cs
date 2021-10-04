using Unity.VisualScripting.Community.Utility;
using System;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    [RenamedFrom("Bolt.Addons.Community.Code.ClassConstructorDeclaration")]
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
