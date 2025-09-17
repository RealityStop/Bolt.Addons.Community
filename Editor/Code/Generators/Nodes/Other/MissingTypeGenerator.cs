#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(MissingType))]
    public class MissingTypeGenerator : NodeGenerator<MissingType>
    {
        public MissingTypeGenerator(Unit unit) : base(unit) { }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return $"/* The type {Unit.formerType} could not be found */".WarningHighlight();
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return $"/* The type {Unit.formerType} could not be found */".WarningHighlight();
        }
    }
}
#endif