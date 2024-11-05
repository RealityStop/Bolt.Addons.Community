using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community 
{
    public class SnippetHandler
    {
        public SnippetType snippetType { get; private set; }
        public Unit originalUnit { get; private set; }
    
        public SnippetHandler(SnippetType snippetType, Unit originalUnit)
        {
            this.snippetType = snippetType;
            this.originalUnit = originalUnit;
        }
    
        public void AddSnippet(FlowGraph graph, FlowCanvas canvas)
        {
            var sourcePort = canvas.connectionSource;
            var preservation = new UnitPreservationContext(originalUnit, graph, snippetType)
            {
                sourcePort = sourcePort
            };
    
            canvas.CancelConnection();
            preservation.AddToGraph();
            preservation.Connect();
            preservation.Reset();
        }
    } 
}