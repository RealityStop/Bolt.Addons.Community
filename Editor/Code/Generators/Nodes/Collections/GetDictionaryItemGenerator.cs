using System.Collections;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(GetDictionaryItem))]
    public class GetDictionaryItemGenerator : NodeGenerator<GetDictionaryItem>
    {
        public GetDictionaryItemGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            var code = MakeClickableForThisUnit($"[") + GenerateValue(Unit.key, data) + MakeClickableForThisUnit("]");
            data.CreateSymbol(Unit, typeof(object));
            data.SetExpectedType(Unit.dictionary.type);
            var dictionaryCode = GenerateValue(Unit.dictionary, data);
            var (type, isMet) = data.RemoveExpectedType();
            if (isMet)
            {
                if (type.IsGenericType && typeof(IDictionary).IsAssignableFrom(type))
                    data.SetSymbolType(Unit, type.GetGenericArguments()[1]);
            }
            return Unit.CreateClickableString().Ignore(dictionaryCode + code).Cast(data.GetExpectedType(), data.GetExpectedType() != null && !data.IsCurrentExpectedTypeMet() && !(data.TryGetSymbol(Unit, out var symbol) && data.GetExpectedType().IsAssignableFrom(symbol.Type)));
        }
    }
}
