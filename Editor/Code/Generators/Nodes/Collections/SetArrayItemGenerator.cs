using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(SetArrayItem))]
    public class SetArrayItemGenerator : NodeGenerator<SetArrayItem>
    {
        public SetArrayItemGenerator(Unit unit) : base(unit) { }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            writer.WriteIndented();
            GenerateValue(Unit.array, data, writer);

            writer.Write("[");
            GenerateIndexes(Unit.indexes, data, writer);
            writer.Write("]");

            writer.Equal();
            GenerateValue(Unit.value, data, writer);

            writer.WriteEnd(EndWriteOptions.LineEnd);

            GenerateExitControl(Unit.exit, data, writer);
        }

        private void GenerateIndexes(List<ValueInput> indexes, ControlGenerationData data, CodeWriter writer)
        {
            for (int i = 0; i < indexes.Count; i++)
            {
                if (i > 0)
                    writer.ParameterSeparator();

                GenerateValue(indexes[i], data, writer);
            }
        }
    }
}