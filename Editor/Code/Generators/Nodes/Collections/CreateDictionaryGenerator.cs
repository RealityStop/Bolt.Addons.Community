using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(CreateDictionary))]
    public class CreateDictionaryGenerator : NodeGenerator<CreateDictionary>
    {
        public CreateDictionaryGenerator(Unit unit) : base(unit) { }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            var type = GetDictionaryType(data);
            var expectedType = data.GetExpectedType();
            if (expectedType != null && expectedType.IsAssignableFrom(type))
            {
                data.MarkExpectedTypeMet(type);
            }
            writer.New(type);
        }

        Type GetDictionaryType(ControlGenerationData data)
        {
            if (data.GetExpectedType() == typeof(System.Collections.IDictionary) || data.GetExpectedType() == typeof(object))
            {
                return typeof(AotDictionary);
            }
            else return data.GetExpectedType() ?? typeof(AotDictionary);
        }
    }
}
