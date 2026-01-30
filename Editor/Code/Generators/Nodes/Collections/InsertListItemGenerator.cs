using System;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(InsertListItem))]
    public sealed class InsertListItemGenerator : NodeGenerator<InsertListItem>
    {
        public InsertListItemGenerator(Unit unit) : base(unit) { }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            writer.WriteIndented();
            GenerateValue(Unit.listInput, data, writer);

            var sourceType = GetSourceType(Unit.listInput, data, writer, false);
            var isGenericList = sourceType.IsGenericType && sourceType.GetGenericTypeDefinition() == typeof(List<>);

            IDisposable ExpectedTypeScope = null;

            if (isGenericList)
                ExpectedTypeScope = data.Expect(sourceType.GetGenericArguments()[0]);

            writer.Write(".Insert(");

            GenerateValue(Unit.index, data, writer);

            writer.ParameterSeparator();

            GenerateValue(Unit.item, data, writer);

            writer.WriteEnd();

            if (isGenericList)
                ExpectedTypeScope.Dispose();

            GenerateExitControl(Unit.exit, data, writer);
        }
    }
}
