namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(NullCoalesce))]
    public sealed class NullCoalesceGenerator : NodeGenerator<NullCoalesce>
    {
        public NullCoalesceGenerator(NullCoalesce unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            var inputCode = GenerateValue(Unit.input, data);
            var fallbackCode = GenerateValue(Unit.fallback, data);

            var result = $"{inputCode}{MakeClickableForThisUnit(" ?? ")}{fallbackCode}";
            return result;
        }
    }
}