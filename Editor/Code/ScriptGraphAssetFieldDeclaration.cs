using System;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    [Inspectable]
    public sealed class ScriptGraphFieldDeclaration : FieldDeclaration
    {
        public object defaultValue;
    }
}
