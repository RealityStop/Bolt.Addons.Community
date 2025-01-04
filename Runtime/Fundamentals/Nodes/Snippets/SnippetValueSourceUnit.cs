using System;

namespace Unity.VisualScripting.Community 
{
    [TypeIcon(typeof(GraphOutput))]
    public class SnippetValueSourceUnit : SnippetSourceUnit
    {
        [Obsolete(Serialization.ConstructorWarning)]
        public SnippetValueSourceUnit() { }
        public SnippetValueSourceUnit(Type sourceType)
        {
            this.sourceType = sourceType;
        }
        [PortLabelHidden]
        [DoNotSerialize]
        public ValueInput source { get; private set; }
    
        public Type sourceType;
    
        protected override void Definition()
        {
            isControlRoot = true;
    
            source = ValueInput(sourceType, nameof(source));
        }
    } 
}