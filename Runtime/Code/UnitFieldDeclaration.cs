using System;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    [Inspectable]
    public sealed class UnitFieldDeclaration : FieldDeclaration
    {
        public object defaultValue;
    }
}
