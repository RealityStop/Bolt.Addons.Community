using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(CopyToClipboardUnit))]
    public class CopyToClipboardUnitGenerator : NodeGenerator<CopyToClipboardUnit>
    {
        public CopyToClipboardUnitGenerator(Unit unit) : base(unit) { }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            writer.WriteIndented("GUIUtility".TypeHighlight() + "." + "systemCopyBuffer".VariableHighlight() + " = ");
            GenerateValue(Unit.text, data, writer);
            writer.Write(";");
            writer.NewLine();
            GenerateChildControl(Unit.exit, data, writer);
        }
    }
}