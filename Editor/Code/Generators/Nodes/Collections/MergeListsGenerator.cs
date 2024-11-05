using Unity.VisualScripting;
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.Humility;
using System.Collections;

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
                return MakeSelectableForThisUnit("CSharpUtility".TypeHighlight() + $".MergeLists<{GetExpectedType(data.GetExpectedType()).As().CSharpName(false, true)}>(") + $"{string.Join(MakeSelectableForThisUnit(", "), Unit.multiInputs.Select(input => GenerateValue(input, data)))}{MakeSelectableForThisUnit(")")}";
            }
            else
                return  MakeSelectableForThisUnit("CSharpUtility".TypeHighlight() + $".MergeLists(") + $"{string.Join(MakeSelectableForThisUnit(", "), Unit.multiInputs.Select(input => GenerateValue(input, data)))}{MakeSelectableForThisUnit(")")}";
        }

        private Type GetExpectedType(Type type)
        {
            if (typeof(IList).IsAssignableFrom(type))
            {
                if (type.IsGenericType)
                {
                    return type.GetGenericArguments()[0];
                }

                if (type == typeof(AotList))
                {
                    return typeof(object);
                }
            }
            return null;
        }
    }
}