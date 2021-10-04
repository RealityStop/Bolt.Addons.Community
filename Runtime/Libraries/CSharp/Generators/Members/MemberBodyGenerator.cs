using System;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.MemberBodyGenerator")]
    public abstract class MemberBodyGenerator : BodyGenerator
    {
        public string name;
        public AccessModifier scope;
        public Type returnType;
    }
}
