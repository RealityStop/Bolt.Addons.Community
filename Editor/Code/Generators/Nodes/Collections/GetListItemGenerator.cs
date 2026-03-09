using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(GetListItem))]
    public class GetListItemGenerator : NodeGenerator<GetListItem>
    {
        private static Dictionary<Type, Type> typeCache = new Dictionary<Type, Type>();

        public GetListItemGenerator(Unit unit) : base(unit) { }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            Type expectedType = data.GetExpectedType();

            data.CreateSymbol(Unit, typeof(object));

            ExpectedTypeResult result;
            using (data.Expect(Unit.list.type, out result))
            {
                GenerateValue(Unit.list, data, writer);
            }

            writer.Write("[");
            using (data.Expect(typeof(int)))
            {
                GenerateValue(Unit.index, data, writer);
            }
            writer.Write("]");

            Type type = result.ResolvedType;
            bool satisfied = result.IsSatisfied;

            Type collectionType = GetCollectionType(type);

            if (satisfied)
            {
                data.SetSymbolType(Unit, collectionType);
            }

            if (collectionType != null && collectionType != typeof(object) && expectedType != null && expectedType != typeof(object))
            {
                bool met = collectionType.IsAssignableFrom(expectedType);
                if (met)
                    data.MarkExpectedTypeMet(collectionType);
            }

            bool expectedTypeNotMet = expectedType != null && !data.IsCurrentExpectedTypeMet() && !expectedType.IsStrictlyAssignableFrom(collectionType);

            if (expectedType != null && expectedTypeNotMet)
            {
                data.MarkExpectedTypeMet(expectedType);
                data.SetSymbolType(Unit, expectedType);
                writer.WriteConvertTo(expectedType, true);
            }
        }

        protected override void GenerateValueInternal(ValueInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input.hasValidConnection)
            {
                GenerateConnectedValue(input, data, writer, false);
                return;
            }

            if (input.hasDefaultValue)
            {
                WriteDefaultValue(input, data, writer);
                return;
            }

            writer.Error($"\"{input.key} Requires Input\"");
        }

        private Type GetCollectionType(Type type)
        {
            if (typeCache.TryGetValue(type, out Type cachedType))
            {
                return cachedType;
            }

            if (type != null && type.IsGenericType && typeof(IList).IsAssignableFrom(type))
            {
                Type genericType = type.GetGenericArguments()[0];
                typeCache[type] = genericType;
                return genericType;
            }

            if (type != null && type.IsArray)
            {
                Type elementType = type.GetElementType();
                typeCache[type] = elementType;
                return elementType;
            }

            return typeof(object);
        }
    }
}
