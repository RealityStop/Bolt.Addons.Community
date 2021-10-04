using System;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.MemberGenerator")]
    public abstract class MemberGenerator : ConstructGenerator
    {
        public string name;
        public AccessModifier scope;
        public Type returnType;
    }
}
