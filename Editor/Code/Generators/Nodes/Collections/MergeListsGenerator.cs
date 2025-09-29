using Unity.VisualScripting;
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.Humility;
using System.Collections;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(MergeLists))]
    public class MergeListsGenerator : NodeGenerator<MergeLists>
    {
        public MergeListsGenerator(Unit unit) : base(unit)
        {
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {

            if (data.GetExpectedType() != null && GetExpectedType(data.GetExpectedType()) != null)
            {
                return MakeClickableForThisUnit("CSharpUtility".TypeHighlight() + $".MergeLists<{GetExpectedType(data.GetExpectedType()).As().CSharpName(false, true)}>(") + $"{string.Join(MakeClickableForThisUnit(", "), Unit.multiInputs.Select(input => GenerateValue(input, data)))}{MakeClickableForThisUnit(")")}";
            }
            else
                return MakeClickableForThisUnit("CSharpUtility".TypeHighlight() + $".MergeLists(") + $"{string.Join(MakeClickableForThisUnit(", "), Unit.multiInputs.Select(input => GenerateValue(input, data)))}{MakeClickableForThisUnit(")")}";
        }

        private Type GetExpectedType(Type type)
        {
            if (typeof(IList).IsAssignableFrom(type) || typeof(IList<>).IsAssignableFrom(type))
            {
                if (type.IsGenericType)
                {
                    return type.GetGenericArguments()[0];
                }

                if (type == typeof(AotList))
                {
                    return typeof(object);
                }
                NameSpaces = type.Namespace;
            }
            return null;
        }
    }
}