using System;

namespace Unity.VisualScripting.Community
{
    [TypeIcon(typeof(Type))]
    [SpecialUnit]
    public class GenericNode : Unit
    {
        [Obsolete(Serialization.ConstructorWarning)]
        public GenericNode() { }
        public GenericNode(MethodDeclaration method, int id)
        {
            this.Method = method;
        }
        public GenericParameter genericParameter => Method.genericParameters[Id];
        public int Id { get; private set; }
        public MethodDeclaration Method { get; private set; }
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput result;
        protected override void Definition()
        {
            result = ValueOutput<Type>(nameof(result));
        }
    }
}