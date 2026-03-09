using System;
using System.Collections;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(MergeLists))]
    public class MergeListsGenerator : NodeGenerator<MergeLists>
    {
        public MergeListsGenerator(Unit unit) : base(unit)
        {
        }

        public override IEnumerable<string> GetNamespaces()
        {
            yield return "Unity.VisualScripting.Community";
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            var expected = data.GetExpectedType();
            var genericType = expected != null ? GetExpectedType(expected) : null;

            if (expected != null && genericType != null)
            {
                writer.CallCSharpUtilityGenericMethod(nameof(CSharpUtility.MergeLists),
                    new CodeWriter.TypeParameter[] { new CodeWriter.TypeParameter(true) { TypeValue = genericType } },
                    writer.Action(() =>
                    {
                        for (int i = 0; i < Unit.multiInputs.Count; i++)
                        {
                            GenerateValue(Unit.multiInputs[i], data, writer);
                            if (i < Unit.multiInputs.Count - 1)
                                writer.ParameterSeparator();
                        }
                    })
                );
            }
            else
            {
                writer.CallCSharpUtilityMethod(nameof(CSharpUtility.MergeLists),
                    writer.Action(() =>
                    {
                        for (int i = 0; i < Unit.multiInputs.Count; i++)
                        {
                            GenerateValue(Unit.multiInputs[i], data, writer);
                            if (i < Unit.multiInputs.Count - 1)
                                writer.ParameterSeparator();
                        }
                    }
                ));
            }
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
            }
            return null;
        }
    }
}