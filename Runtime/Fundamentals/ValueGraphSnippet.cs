using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [CreateAssetMenu(menuName = "Visual Scripting/Community/Snippets/Value Snippet")]
    [TypeIcon(typeof(FlowGraph))]
    public sealed class ValueGraphSnippet : GraphSnippet
    {
        public sealed override SnippetType SnippetType => SnippetType.ValueInput;
        [HideInInspector]
        public SystemType sourceType = new(typeof(object));
        [SerializeField]
        [HideInInspector]
        private SnippetValueSourceUnit _sourceUnit;
        public SnippetValueSourceUnit sourceUnit { get => _sourceUnit; private set => _sourceUnit = value; }
        public sealed override Unit SourceUnit()
        {
            if (sourceUnit == null)
            {
                sourceUnit = new SnippetValueSourceUnit(sourceType.type);
            }

            return sourceUnit;
        }
    }
}