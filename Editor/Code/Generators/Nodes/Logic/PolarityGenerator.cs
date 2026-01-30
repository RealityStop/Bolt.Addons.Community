using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Polarity))]
    public class PolarityGenerator : NodeGenerator<Polarity>
    {
        public PolarityGenerator(Unit unit) : base(unit) { }

        public override IEnumerable<string> GetNamespaces()
        {
            yield return "UnityEngine";
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (output == Unit.positive)
            {
                GenerateValue(Unit.input, data, writer);
                writer.Write(" > " + "0".NumericHighlight());
            }
            else if (output == Unit.negative)
            {
                GenerateValue(Unit.input, data, writer);
                writer.Write(" < " + "0".NumericHighlight());
            }
            else if (output == Unit.zero)
            {
                writer.Write("Mathf".TypeHighlight() + ".Approximately(");
                GenerateValue(Unit.input, data, writer);
                writer.Write(", " + "0f".NumericHighlight() + ")");
            }
        }
    }
}
