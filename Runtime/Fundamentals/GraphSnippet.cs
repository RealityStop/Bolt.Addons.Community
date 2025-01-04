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
        [InspectorWide]
        public List<SnippetArgument> snippetArguments = new List<SnippetArgument>();
        public abstract SnippetType SnippetType { get; }

        public bool graphContainsUnit => graph.units.Contains(SourceUnit());
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

        [Inspectable]
        [Serializable]
        public class SnippetArgument
        {
            [Inspectable]
            public string argumentName = "arg";
            [Inspectable]
            [TypeFilter(TypesMatching.Any, typeof(string), typeof(int), typeof(float), typeof(double), typeof(decimal), typeof(bool), typeof(char), typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(uint), typeof(long), typeof(ulong), typeof(LayerMask))]
            [TypeSet(TypeSet.SettingsAssembliesTypes)]
            public Type argumentType = typeof(string);
        }
    }

    public enum SnippetType
    {
        ControlInput,
        ValueInput
    }
}
