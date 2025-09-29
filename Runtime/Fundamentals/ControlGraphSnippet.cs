using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [CreateAssetMenu(menuName = "Visual Scripting/Community/Snippets/Control Snippet")]
    [TypeIcon(typeof(FlowGraph))]
    public sealed class ControlGraphSnippet : GraphSnippet
    {
        public sealed override SnippetType SnippetType => SnippetType.ControlInput;
        private SnippetControlSourceUnit _sourceUnit;
        public sealed override Unit SourceUnit()
        {
            if (_sourceUnit == null)
            {
                _sourceUnit = new SnippetControlSourceUnit();
            }
            return _sourceUnit;
        }
    }
}