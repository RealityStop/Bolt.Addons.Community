
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Using))]
    public class UsingGenerator : NodeGenerator<Using>
    {
        public UsingGenerator(Unit unit) : base(unit) { }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            writer.WriteIndented();
            writer.Write("using".ConstructHighlight());
            writer.Write(" (");
            GenerateValue(Unit.value, data, writer);
            writer.Write(")");
            writer.NewLine();
            writer.WriteLine("{");
            using (writer.Indented())
            {
                GenerateChildControl(Unit.body, data, writer);
            }
            writer.WriteLine("}");
            GenerateExitControl(Unit.exit, data, writer);
        }
    }
}
