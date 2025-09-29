using System;
using System.Collections;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(GetListItem))]
    public class GetListItemGenerator : NodeGenerator<GetListItem>
    {
        private Dictionary<Type, Type> typeCache = new Dictionary<Type, Type>();
        public GetListItemGenerator(Unit unit) : base(unit)
        {
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            var code = MakeClickableForThisUnit($"[") + GenerateValue(Unit.index, data) + MakeClickableForThisUnit("]");
            data.CreateSymbol(Unit, typeof(object));
            data.SetExpectedType(Unit.list.type);
            var listCode = GenerateValue(Unit.list, data);
            var (type, isMet) = data.RemoveExpectedType();
            if (isMet)
            {
                data.SetSymbolType(Unit, GetCollectionType(type));
            }
            var collectionType = GetCollectionType(type);
            if (collectionType != null && collectionType != typeof(object) && data.GetExpectedType() != null && data.GetExpectedType() != typeof(object))
            {
                var _isMet = data.GetExpectedType().IsAssignableFrom(collectionType);
                data.SetCurrentExpectedTypeMet(_isMet, collectionType);
            }

            return Unit.CreateIgnoreString(listCode + code).EndIgnoreContext().Cast(data.GetExpectedType(), data.GetExpectedType() != null && !data.IsCurrentExpectedTypeMet() && !data.GetExpectedType().IsAssignableFrom(GetCollectionType(type)));
        }

        private Type GetCollectionType(Type type)
        {
            if (typeCache.TryGetValue(type, out var cachedType))
            {
                return cachedType;
            }
            if (type != null && type.IsGenericType && typeof(IList).IsAssignableFrom(type))
            {
                var genericType = type.GetGenericArguments()[0];
                typeCache[type] = genericType;
                return genericType;
            }
            else if (type != null && type.IsArray)
            {
                var elementType = type.GetElementType();
                typeCache[type] = elementType;
                return elementType;
            }
            else
                return typeof(object);
        }
    }
}