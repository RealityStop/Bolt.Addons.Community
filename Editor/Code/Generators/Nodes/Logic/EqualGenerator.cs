using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Equal))]
    public sealed class EqualGenerator : NodeGenerator<Equal>
    {
        public EqualGenerator(Equal unit) : base(unit)
        {
        }

        protected override void GenerateValueInternal(ValueInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input == Unit.a)
            {
                if (Unit.a.hasValidConnection)
                {
                    IDisposable expectScope = null;
                    if (Unit.b.hasValidConnection && NodeGeneration.IsSourceLiteral(Unit.b, out var type))
                    {
                        expectScope = data.Expect(type);
                    }

                    GenerateConnectedValue(Unit.a, data, writer, false);

                    expectScope?.Dispose();
                    return;
                }
            }

            if (input == Unit.b)
            {
                if (Unit.b.hasAnyConnection)
                {
                    IDisposable expectScope = null;
                    if (Unit.a.hasValidConnection && NodeGeneration.IsSourceLiteral(Unit.a, out var type))
                    {
                        expectScope = data.Expect(type);
                    }

                    GenerateConnectedValue(Unit.b, data, writer, false);

                    expectScope?.Dispose();
                    return;
                }
                else if (Unit.numeric)
                {
                    writer.Write(Unit.defaultValues["b"].As().Code(true));
                    return;
                }
            }

            base.GenerateValueInternal(input, data, writer);
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (output == Unit.comparison)
            {
                GenerateValue(Unit.a, data, writer);
                writer.Write(" == ");
                GenerateValue(Unit.b, data, writer);
            }
        }
    }
}