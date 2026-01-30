using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    public abstract class ModuloGenerator<T> : NodeGenerator<Modulo<T>>
    {
        public ModuloGenerator(Unit unit) : base(unit) { }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            using (data.Expect(typeof(int)))
            {
                GenerateValue(Unit.dividend, data, writer);
            }
            writer.Write(" % ");
            using (data.Expect(typeof(int)))
            {
                GenerateValue(Unit.divisor, data, writer);
            }
        }

        protected override void GenerateValueInternal(ValueInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input.hasValidConnection)
            {
                GenerateConnectedValue(input, data, writer);
            }
            else if (input.hasDefaultValue)
            {
                if (data.GetExpectedType() == typeof(int))
                {
                    writer.Write(int.Parse(unit.defaultValues[input.key].ToString()).As().Code(true, true, true, "", false));
                }
                else
                {
                    writer.Write(unit.defaultValues[input.key].As().Code(true, true, true, "", false));
                }
            }
            else
            {
                writer.Write($"/* \"{input.key} Requires Input\" */".ErrorHighlight());
            }
        }
    }
}
