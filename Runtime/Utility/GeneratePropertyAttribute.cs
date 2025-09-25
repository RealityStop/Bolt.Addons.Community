using System;

namespace Unity.VisualScripting.Community 
{
    /// <summary>
    /// Used by the code generator to force the property to be generated.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class GeneratePropertyAttribute : Attribute
    {
    } 
}
