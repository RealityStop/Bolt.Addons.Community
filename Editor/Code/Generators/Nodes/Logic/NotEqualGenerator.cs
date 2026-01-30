using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(NotEqual))]
    public sealed class NotEqualGenerator : NodeGenerator<NotEqual>
    {
        public NotEqualGenerator(NotEqual unit) : base(unit)
        {
        }

        protected override void GenerateValueInternal(ValueInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input == Unit.a)
            {
                if (Unit.a.hasAnyConnection)
                {
                    IDisposable expectScope = null;
                    if (Unit.b.hasValidConnection && Unit.b.GetPesudoSource()?.unit is Literal literal)
                    {
                        expectScope = data.Expect(literal.type);
                    }
                    base.GenerateValueInternal(Unit.a, data, writer);
                    if (Unit.b.hasValidConnection && Unit.b.GetPesudoSource()?.unit is Literal)
                    {
                        expectScope?.Dispose();
                    }
                    return;
                }
            }

            if (input == Unit.b)
            {
                if (Unit.b.hasAnyConnection)
                {
                    IDisposable expectScope = null;
                    if (Unit.a.hasValidConnection && Unit.a.GetPesudoSource()?.unit is Literal literal)
                    {
                        expectScope = data.Expect(literal.type);
                    }
                    base.GenerateValueInternal(Unit.b, data, writer);
                    if (Unit.a.hasValidConnection && Unit.a.GetPesudoSource()?.unit is Literal)
                    {
                        expectScope?.Dispose();
                    }
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
                writer.Write(" != ");
                GenerateValue(Unit.b, data, writer);
            }
        }
    }
}