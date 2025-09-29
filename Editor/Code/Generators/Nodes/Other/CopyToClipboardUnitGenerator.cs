using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(CopyToClipboardUnit))]
    public class CopyToClipboardUnitGenerator : NodeGenerator<CopyToClipboardUnit>
    {
        public CopyToClipboardUnitGenerator(Unit unit) : base(unit) { }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return base.GenerateValue(output, data);
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = "";
            output += Unit.CreateClickableString(CodeBuilder.Indent(indent)).SetMember(typeof(GUIUtility), "systemCopyBuffer", value => value.Ignore(GenerateValue(Unit.text, data))).Clickable(";").NewLine().Ignore(GetNextUnit(Unit.exit, data, indent));
            return output;
        }
    }
}