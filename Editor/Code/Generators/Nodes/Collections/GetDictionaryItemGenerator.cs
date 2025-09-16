using System.Collections;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(GetDictionaryItem))]
    public class GetDictionaryItemGenerator : NodeGenerator<GetDictionaryItem>
    {
        public GetDictionaryItemGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            var code = MakeSelectableForThisUnit($"[") + GenerateValue(Unit.key, data) + MakeSelectableForThisUnit("]");
            data.CreateSymbol(Unit, typeof(object), code);
            data.SetExpectedType(Unit.dictionary.type);
            var dictionaryCode = GenerateValue(Unit.dictionary, data);
            var (type, isMet) = data.RemoveExpectedType();
            if (isMet)
            {
                if (type.IsGenericType && typeof(IDictionary).IsAssignableFrom(type))
                    data.SetSymbolType(Unit, type.GetGenericArguments()[1]);
            }
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
            return new ValueCode(dictionaryCode + code, data.GetExpectedType(), data.GetExpectedType() != null && !data.IsCurrentExpectedTypeMet() && !(data.TryGetSymbol(Unit, out var symbol) && data.GetExpectedType().IsAssignableFrom(symbol.Type)));
=======
            return Unit.CreateClickableString().Ignore(dictionaryCode + code).Cast(data.GetExpectedType(), data.GetExpectedType() != null && !data.IsCurrentExpectedTypeMet() && !(data.TryGetSymbol(Unit, out var symbol) && data.GetExpectedType().IsAssignableFrom(symbol.Type)));
>>>>>>> Stashed changes
=======
            return Unit.CreateClickableString().Ignore(dictionaryCode + code).Cast(data.GetExpectedType(), data.GetExpectedType() != null && !data.IsCurrentExpectedTypeMet() && !(data.TryGetSymbol(Unit, out var symbol) && data.GetExpectedType().IsAssignableFrom(symbol.Type)));
>>>>>>> Stashed changes
=======
            return Unit.CreateClickableString().Ignore(dictionaryCode + code).Cast(data.GetExpectedType(), data.GetExpectedType() != null && !data.IsCurrentExpectedTypeMet() && !(data.TryGetSymbol(Unit, out var symbol) && data.GetExpectedType().IsAssignableFrom(symbol.Type)));
>>>>>>> Stashed changes
        }
    }
}
