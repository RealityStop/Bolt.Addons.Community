using System;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community
{
    public sealed class CastSourceNode : SourceNode
    {
        public Type CastType;
        public Func<bool> ShouldCast;
        public bool WrapInParentheses;

        public CastSourceNode(SourceSpan span, Unit unit, SourceNode parent, Type castType, Func<bool> shouldCast, bool wrapInParentheses) : base(span, unit, parent)
        {
            CastType = castType;
            ShouldCast = shouldCast;
            WrapInParentheses = wrapInParentheses;
        }
    }
}