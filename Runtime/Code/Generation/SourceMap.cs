using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Unity.VisualScripting.Community 
{
    public sealed class SourceMap
    {
        private readonly List<SourceNode> nodes = new List<SourceNode>();

        public ReadOnlyCollection<SourceNode> Nodes => nodes.AsReadOnly();
    
        public void Register(SourceNode node)
        {
            nodes.Add(node);
        }

        public void Register(Unit unit, int start, int end, SourceNode parent)
        {
            nodes.Add(new SourceNode(new SourceSpan(start, end), unit, parent));
        }
    
        /// <summary>
        /// Returns the most specific node at index
        /// </summary>
        public SourceNode Resolve(int index)
        {
            SourceNode best = null;
    
            foreach (var node in nodes)
            {
                if (!node.Span.Contains(index))
                    continue;
    
                if (best == null || node.Span.Length < best.Span.Length)
                    best = node;
            }
    
            return best;
        }
    } 
}