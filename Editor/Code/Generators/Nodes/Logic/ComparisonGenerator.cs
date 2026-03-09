using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Unity.VisualScripting.Comparison))]
    public class ComparisonGenerator : NodeGenerator<Comparison>
    {
        public ComparisonGenerator(Unit unit) : base(unit) { }

        public override IEnumerable<string> GetNamespaces()
        {
            yield return "UnityEngine";
            yield return "Unity.VisualScripting";
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            bool numeric = Unit.numeric;

            if (numeric)
            {
                if (output == Unit.aLessThanB)
                {
                    GenerateValue(Unit.a, data, writer);
                    writer.Write(" < ");
                    GenerateValue(Unit.b, data, writer);
                }
                else if (output == Unit.aLessThanOrEqualToB)
                {
                    GenerateValue(Unit.a, data, writer);
                    writer.Write(" < ");
                    GenerateValue(Unit.b, data, writer);
                    writer.Write(" || " + "Mathf".TypeHighlight() + ".Approximately(");
                    GenerateValue(Unit.a, data, writer);
                    writer.Write(", ");
                    GenerateValue(Unit.b, data, writer);
                    writer.Write(")");
                }
                else if (output == Unit.aEqualToB)
                {
                    writer.Write("Mathf".TypeHighlight() + ".Approximately(");
                    GenerateValue(Unit.a, data, writer);
                    writer.Write(", ");
                    GenerateValue(Unit.b, data, writer);
                    writer.Write(")");
                }
                else if (output == Unit.aNotEqualToB)
                {
                    writer.Write("!" + "Mathf".TypeHighlight() + ".Approximately(");
                    GenerateValue(Unit.a, data, writer);
                    writer.Write(", ");
                    GenerateValue(Unit.b, data, writer);
                    writer.Write(")");
                }
                else if (output == Unit.aGreaterThanOrEqualToB)
                {
                    GenerateValue(Unit.a, data, writer);
                    writer.Write(" > ");
                    GenerateValue(Unit.b, data, writer);
                    writer.Write(" || " + "Mathf".TypeHighlight() + ".Approximately(");
                    GenerateValue(Unit.a, data, writer);
                    writer.Write(", ");
                    GenerateValue(Unit.b, data, writer);
                    writer.Write(")");
                }
                else if (output == Unit.aGreatherThanB)
                {
                    GenerateValue(Unit.a, data, writer);
                    writer.Write(" > ");
                    GenerateValue(Unit.b, data, writer);
                }
            }
            else
            {
                writer.Write("OperatorUtility".TypeHighlight() + ".");
                if (output == Unit.aLessThanB)
                {
                    writer.Write("LessThan(");
                }
                else if (output == Unit.aLessThanOrEqualToB)
                {
                    writer.Write("LessThanOrEqual(");
                }
                else if (output == Unit.aEqualToB)
                {
                    writer.Write("Equal(");
                }
                else if (output == Unit.aNotEqualToB)
                {
                    writer.Write("NotEqual(");
                }
                else if (output == Unit.aGreaterThanOrEqualToB)
                {
                    writer.Write("GreaterThanOrEqual(");
                }
                else if (output == Unit.aGreatherThanB)
                {
                    writer.Write("GreaterThan(");
                }
                GenerateValue(Unit.a, data, writer);
                writer.Write(", ");
                GenerateValue(Unit.b, data, writer);
                writer.Write(")");
            }
        }
    }
}