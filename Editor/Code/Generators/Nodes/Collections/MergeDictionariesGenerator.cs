using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(MergeDictionaries))]
    public class MergeDictionariesGenerator : NodeGenerator<MergeDictionaries>
    {
        public MergeDictionariesGenerator(Unit unit) : base(unit) { NameSpaces = "Unity.VisualScripting.Community"; }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (data.GetExpectedType() != null && GetExpectedType(data.GetExpectedType()) != null)
            {
                return CodeBuilder.CallCSharpUtilityGenericMethod(Unit, MakeClickableForThisUnit(nameof(CSharpUtility.MergeDictionaries)), Unit.multiInputs.Select(input => GenerateValue(input, data)).ToArray(), GetExpectedType(data.GetExpectedType()));
            }
            else
                return CodeBuilder.CallCSharpUtilityMethod(Unit, MakeClickableForThisUnit(nameof(CSharpUtility.MergeDictionaries)), Unit.multiInputs.Select(input => GenerateValue(input, data)).ToArray());
        }

        private Type[] GetExpectedType(Type type)
        {
            if (typeof(IDictionary).IsAssignableFrom(type) || typeof(IDictionary<,>).IsAssignableFrom(type))
            {
                NameSpaces += "," + type.Namespace;
                if (type.IsGenericType)
                {
                    var types = type.GetGenericArguments();
                    NameSpaces += "," + string.Join(",", types.Select(t => t.Namespace));
                    return types;
                }
                else if (type == typeof(AotDictionary))
                {
                    return new Type[] { typeof(object), typeof(object) };
                }
            }
            return new Type[] { typeof(object), typeof(object) };
        }
    }
}
