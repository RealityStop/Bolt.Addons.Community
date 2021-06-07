using System;

namespace Bolt.Addons.Libraries.CSharp
{
    public abstract class MemberGenerator : ConstructGenerator
    {
        public string name;
        public AccessModifier scope;
        public Type returnType;
    }
}
