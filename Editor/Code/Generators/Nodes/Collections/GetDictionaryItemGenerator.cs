using System;
using System.Collections;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(GetDictionaryItem))]
    public class GetDictionaryItemGenerator : NodeGenerator<GetDictionaryItem>
    {
        public GetDictionaryItemGenerator(Unit unit) : base(unit) { }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            data.CreateSymbol(Unit, typeof(object));
            ExpectedTypeResult result;
            using (data.Expect(Unit.dictionary.type, out result))
            {
                GenerateValue(Unit.dictionary, data, writer);
            }

            writer.Brackets(w =>
            {
                GenerateValue(Unit.key, data, w);
            });

            bool satisfied = result.IsSatisfied;
            Type type = result.ResolvedType;

            if (satisfied)
            {
                if (type.IsGenericType && typeof(IDictionary).IsAssignableFrom(type))
                    data.SetSymbolType(Unit, type.GetGenericArguments()[1]);
            }

            var expectedType = data.GetExpectedType();
            writer.WriteConvertTo(expectedType, expectedType != null && !data.IsCurrentExpectedTypeMet() && expectedType != typeof(object));
        }

        protected override void GenerateValueInternal(ValueInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input == Unit.dictionary)
            {
                if (input.hasValidConnection)
                    GenerateConnectedValue(input, data, writer, false);
                return;
            }
            base.GenerateValueInternal(input, data, writer);
        }
    }
}