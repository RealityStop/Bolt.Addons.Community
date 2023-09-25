using System;
using Unity.VisualScripting.Community;

[AttributeUsage(AttributeTargets.Property)]
public class IDefinedEventFilterAttribute : Attribute
{
    public bool AllowsType(Type type)
    {
        return typeof(IDefinedEvent).IsAssignableFrom(type);
    }
}
