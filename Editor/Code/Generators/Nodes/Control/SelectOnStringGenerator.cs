using System;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(SelectOnString))]
    public sealed class SelectOnStringGenerator : NodeGenerator<SelectOnString>
    {
        public SelectOnStringGenerator(SelectOnString unit) : base(unit) { }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            GenerateValue(Unit.selector, data, writer);
            if (Unit.ignoreCase)
            {
                writer.Write(".ToLowerInvariant()");
            }
            writer.Write(" switch".ControlHighlight());
            writer.NewLine();
            writer.WriteLine("{");
            
            using (writer.Indented())
            {
                foreach (var branch in Unit.branches)
                {
                    var caseKey = Unit.ignoreCase ? branch.Key.ToLowerInvariant() ?? "null" : branch.Key;
                    writer.WriteIndented();
                    writer.Object(caseKey);
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