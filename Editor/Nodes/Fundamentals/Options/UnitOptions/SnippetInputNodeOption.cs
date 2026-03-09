using System;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [FuzzyOption(typeof(SnippetInputNode))]
    public class SnippetInputNodeOption : UnitOption<SnippetInputNode>
    {
        [Obsolete(Serialization.ConstructorWarning)]
        public SnippetInputNodeOption() : base() { }

        public SnippetInputNodeOption(SnippetInputNode snippetInputNode) : base(snippetInputNode) { }

        public override bool favoritable => false;

        protected override string Label(bool human)
        {
            if (string.IsNullOrEmpty(unit.argumentName))
                return "Argument";
            else
                return "Argument " + $"<color=#{ColorPalette.unityForegroundDim.ToHexString()}>" + unit.argumentName + "</color>";
        }
    }
}