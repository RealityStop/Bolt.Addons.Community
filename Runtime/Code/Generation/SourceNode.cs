using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class SourceNode
    {
        public SourceSpan Span;
        public readonly Unit Unit;
        public SourceNode Parent;
        public readonly List<SourceNode> Children = new List<SourceNode>();

        public SourceNode(SourceSpan span, Unit unit, SourceNode parent)
        {
            Span = span;
            Unit = unit;
            Parent = parent;
            parent?.Children.Add(this);
        }

        public void Offset(int amount)
        {
            Span = new SourceSpan(Span.Start + amount, Span.End + amount);
            foreach (var child in Children)
            {
                child.Offset(amount);
            }
        }
    }
}