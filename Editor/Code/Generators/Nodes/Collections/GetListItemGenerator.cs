using System.Collections;

namespace Unity.VisualScripting.Community 
{
    [NodeGenerator(typeof(GetListItem))]
    public class GetListItemGenerator : NodeGenerator<GetListItem>
    {
        public GetListItemGenerator(Unit unit) : base(unit)
        {
        }
    
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            var code = MakeSelectableForThisUnit($"[") + GenerateValue(Unit.index, data) + MakeSelectableForThisUnit("]");
            data.CreateSymbol(Unit, typeof(object), code);
            data.SetExpectedType(Unit.list.type);
            var listCode = GenerateValue(Unit.list, data);
            var result = data.RemoveExpectedType();
            if (result.isMet)
            {
                if (result.type.IsGenericType && typeof(IList).IsAssignableFrom(result.type))
                    data.SetSymbolType(Unit, result.type.GetGenericArguments()[0]);
            }
            return new ValueCode(listCode + code, data.GetExpectedType(), data.GetExpectedType() != null && !data.IsCurrentExpectedTypeMet() && !(data.TryGetSymbol(Unit, out var symbol) && data.GetExpectedType().IsAssignableFrom(symbol.Type)));
        }
    } 
}