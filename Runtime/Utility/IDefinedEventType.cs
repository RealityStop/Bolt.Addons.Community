using System;

namespace Unity.VisualScripting.Community
{
    [System.Serializable]
    [Inspectable]
    public class DefinedEventType
    {
        [Inspectable]
        public Type type;

        public DefinedEventType()
        {
        }

        public DefinedEventType(Type type)
        {
            this.type = type;
        }

        public static implicit operator DefinedEventType(Type type)
        {
            return new DefinedEventType(type);
        }

        public static implicit operator Type(DefinedEventType type)
        {
            return type.type;
        }
    }
}//