using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    public abstract class GraphSnippet : Macro<FlowGraph>
    {
        [HideInInspector]
        public string SnippetName = "";
        public abstract SnippetType SnippetType { get; }
        public abstract Unit SourceUnit();
        public sealed override FlowGraph DefaultGraph()
        {
            return new FlowGraph();
        }

        [ContextMenu("Show Data...")]
        protected override void ShowData()
        {
            base.ShowData();
        }
    }

    public enum SnippetType
    {
        ControlInput,
        ValueInput
    }
}
