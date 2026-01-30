using System;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(SelectOnInteger))]
    public sealed class SelectOnIntegerGenerator : NodeGenerator<SelectOnInteger>
    {
        public SelectOnIntegerGenerator(SelectOnInteger unit) : base(unit) { }

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
                    writer.Write(branch.Key.As().Code(false));
                    writer.Write(" => ");
                    GenerateValue(branch.Value, data, writer);
                    writer.Write(",");
                    writer.NewLine();
                }
                
                writer.WriteIndented();
                writer.Write("_".ConstructHighlight() + " => ");
                GenerateValue(Unit.@default, data, writer);
                writer.Write(",");
                writer.NewLine();
            }
            
            writer.WriteIndented("}");
        }
    }
}