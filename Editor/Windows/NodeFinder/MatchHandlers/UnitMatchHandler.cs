using System.Collections.Generic;

namespace Unity.VisualScripting.Community
{
    [MatchHandler]
    public class UnitMatchHandler : UnitBaseMatchHandler
    {
        public override string Name => "Unit";
        public override bool CanHandle(IGraphElement element)
        {
            return element is Unit && !(element is CommentNode);
        }
    }
}