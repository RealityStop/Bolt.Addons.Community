using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(CreateDictionary))]
    public class CreateDictionaryGenerator : NodeGenerator<CreateDictionary>
    {
        public CreateDictionaryGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return MakeClickableForThisUnit("new ".ConstructHighlight() + GetDictionaryType(data).As().CSharpName(false, true) + "()");
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
