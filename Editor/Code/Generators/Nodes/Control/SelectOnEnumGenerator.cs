using System;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(SelectOnEnum))]
    public sealed class SelectOnEnumGenerator : NodeGenerator<SelectOnEnum>
    {
        public SelectOnEnumGenerator(SelectOnEnum unit) : base(unit) { }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            GenerateValue(Unit.selector, data, writer);
            writer.Write(" switch".ControlHighlight());
            writer.NewLine();
            writer.WriteLine("{");
            
            using (writer.Indented())
            {
                foreach (var branch in Unit.branches)
                {
                    writer.WriteIndented();
                    writer.Object(branch.Key);
                    writer.Write(" => ");
                    GenerateValue(branch.Value, data, writer);
                    writer.Write(",");
                    writer.NewLine();
                }
            }
            
            writer.WriteIndented("}");
        }
    }
}