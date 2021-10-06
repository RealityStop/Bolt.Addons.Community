using Unity.VisualScripting.Community.Libraries.Humility;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.TypeGenerator")]
    public abstract class TypeGenerator : BodyGenerator
    {
        protected List<string> usings = new List<string>();

        public virtual TypeGenerator AddUsings(List<string> usings)
        {
            this.usings.MergeUnique(usings);
            return this;
        }
    }
}