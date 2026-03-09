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
        public MergeDictionariesGenerator(Unit unit) : base(unit)
        {
        }

        public override IEnumerable<string> GetNamespaces()
        {
            yield return "Unity.VisualScripting.Community";
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            var expected = data.GetExpectedType();
            var genericTypes = expected != null ? GetExpectedType(expected) : null;

            if (expected != null && genericTypes != null)
            {
                writer.CallCSharpUtilityGenericMethod(nameof(CSharpUtility.MergeDictionaries),
                    genericTypes.Select(t => new CodeWriter.TypeParameter(true) { TypeValue = t }).ToArray(),
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
                writer.CallCSharpUtilityMethod(nameof(CSharpUtility.MergeDictionaries),
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

        private Type[] GetExpectedType(Type type)
        {
            if (typeof(IDictionary).IsAssignableFrom(type) || typeof(IDictionary<,>).IsAssignableFrom(type))
            {
                if (type.IsGenericType)
                {
                    var types = type.GetGenericArguments();

                    return types;
                }

                if (type == typeof(AotDictionary))
                {
                    return new Type[] { typeof(object), typeof(object) };
                }
            }

            return new Type[] { typeof(object), typeof(object) };
        }
    }
}