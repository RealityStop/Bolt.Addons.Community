using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Throw))]
    public class ThrowGenerator : NodeGenerator<Throw>
    {
        public ThrowGenerator(Unit unit) : base(unit)
        {
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            writer.WriteIndented();
            writer.Write("throw".ControlHighlight());
            writer.Space();
            if (Unit.custom)
                GenerateValue(Unit.exception, data, writer);
            else
                GenerateValue(Unit.message, data, writer);
            writer.Write(";");
            writer.NewLine();
        }
    }
}