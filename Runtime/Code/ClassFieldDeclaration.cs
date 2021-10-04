using System;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    [Inspectable]
    [RenamedFrom("Bolt.Addons.Community.Code.ClassFieldDeclaration")]
    public sealed class ClassFieldDeclaration : FieldDeclaration
    {
        public object defaultValue;
    }
}
