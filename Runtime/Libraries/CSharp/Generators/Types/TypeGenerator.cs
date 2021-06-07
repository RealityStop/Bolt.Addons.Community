using Bolt.Addons.Libraries.Humility;
using System.Collections.Generic;

namespace Bolt.Addons.Libraries.CSharp
{
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