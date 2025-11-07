using System;

namespace Unity.VisualScripting.Community.CSharp 
{
    public class LocalVariableGenerator : NodeGenerator
    {
        public Type variableType = typeof(object);
        public LocalVariableGenerator(Unit unit) : base(unit)
        {
        }
    } 
}
