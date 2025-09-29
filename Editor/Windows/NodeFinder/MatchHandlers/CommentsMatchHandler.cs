using System;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community
{
    [MatchHandler]
    public class CommentsMatchHandler : UnitBaseMatchHandler
    {
        public override Type SupportedType => typeof(CommentNode);

        public override string Name => "Comment";

        public override bool CanHandle(IGraphElement element)
        {
            return element is CommentNode;
        }
    }
}