using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(NullCheck))]
    public class NullCheckGenerator : NodeGenerator<NullCheck>
    {
        public NullCheckGenerator(Unit unit) : base(unit)
        {
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (Unit.ifNotNull.hasValidConnection)
            {
                writer.WriteIndented("if".ControlHighlight()).Space().Parentheses(w =>
                {
                    GenerateValue(Unit.input, data, writer);
                    writer.NotEqual().Null();
                }).NewLine();

                writer.WriteLine("{");
                using (writer.IndentedScope(data))
                {
                    GenerateChildControl(Unit.ifNotNull, data, writer);
                }

                writer.WriteLine("}");
                if (Unit.ifNull.hasValidConnection)
                {
                    writer.WriteIndented("else".ControlHighlight()).NewLine();

                    writer.WriteLine("{");
                    using (writer.IndentedScope(data))
                    {
                        GenerateChildControl(Unit.ifNull, data, writer);
                    }
                    writer.WriteLine("}");
                }
            }
            else if (Unit.ifNull.hasValidConnection)
            {
                writer.WriteIndented("if".ControlHighlight()).Space().Parentheses(w =>
                {
                    GenerateValue(Unit.input, data, writer);
                    writer.Equals().Null();
                }).NewLine();

                writer.WriteLine("{");

                using (writer.IndentedScope(data))
                {
                    GenerateChildControl(Unit.ifNull, data, writer);
                }

                writer.WriteLine("}");
            }
        }
    }
}