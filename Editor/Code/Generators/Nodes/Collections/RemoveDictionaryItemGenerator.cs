using System;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(RemoveDictionaryItem))]
    public class RemoveDictionaryItemGenerator : NodeGenerator<RemoveDictionaryItem>
    {
        public RemoveDictionaryItemGenerator(Unit unit) : base(unit) { }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            ExpectedTypeResult result;
            using (data.Expect(typeof(System.Collections.IDictionary), out result))
            {
                writer.WriteIndented();
                GenerateValue(Unit.dictionaryInput, data, writer);
            }
            writer.InvokeMember(null, "Remove", writer.Action(() =>
            {
                if (result.IsSatisfied && typeof(System.Collections.IDictionary).IsAssignableFrom(result.ResolvedType))
                {
                    using (data.Expect(GetKeyExpectedType(result.ResolvedType)))
                    {
                        GenerateValue(Unit.key, data, writer);
                    }
                }
                else
                {
                    GenerateValue(Unit.key, data, writer);
                }
            }));

            writer.WriteEnd(EndWriteOptions.LineEnd);

            GenerateExitControl(Unit.exit, data, writer);
        }

        public Type GetKeyExpectedType(Type type)
        {
            if (type.IsGenericType)
            {
                return type.GetGenericArguments()[0];
            }

            return typeof(object);
        }

    }
}
