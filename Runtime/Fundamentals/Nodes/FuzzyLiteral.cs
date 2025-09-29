using System;

namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// Is a duplicate of the Literal Node but is used for getting the value directly from the fuzzy finder into the node.
    /// </summary>
    [SpecialUnit]
    public sealed class FuzzyLiteral : Unit
    {
        [Obsolete(Serialization.ConstructorWarning)]
        public FuzzyLiteral() : base() { }

        public FuzzyLiteral(Type type) : this(type, type.PseudoDefault()) { }

        public FuzzyLiteral(Type type, object value) : base()
        {
            Ensure.That(nameof(type)).IsNotNull(type);
            Ensure.That(nameof(value)).IsOfType(value, type);

            this.type = type;
            this.value = value;
        }

        public override bool canDefine => type != null;

        [SerializeAs(nameof(value))]
        private object _value;

        [Serialize]
        public Type type { get; internal set; }

        [DoNotSerialize]
        public object value
        {
            get => _value;
            set
            {
                Ensure.That(nameof(value)).IsOfType(value, type);

                _value = value;
            }
        }

        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput output { get; private set; }

        protected override void Definition()
        {
            output = ValueOutput(type, nameof(output), (flow) => value).Predictable();
        }
    }
}