using Unity.VisualScripting.Community.Libraries.Humility;
using System.Collections.Generic;
using Unity.VisualScripting.Community.CSharp;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.TypeGenerator")]
    public abstract class TypeGenerator : BodyGenerator
    {
        public sealed override void InitializeBody(CodeWriter writer)
        {
            CodeWriter.BuildAmbiguityCache();
            writer.includedNamespaces.AddRange(Usings());
        }

        protected List<string> usings = new List<string>();

        public virtual TypeGenerator AddUsings(List<string> usings)
        {
            this.usings.MergeUnique(usings);
            return this;
        }

        public virtual TypeGenerator AddUsings(HashSet<string> usings)
        {
            this.usings.MergeUnique(usings);
            return this;
        }
    }
}