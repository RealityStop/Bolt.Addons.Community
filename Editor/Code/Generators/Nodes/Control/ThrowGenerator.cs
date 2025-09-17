using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Throw))]
    public class ThrowGenerator : NodeGenerator<Throw>
    {
        public ThrowGenerator(Unit unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return CodeBuilder.Indent(indent) + MakeClickableForThisUnit("throw ".ControlHighlight()) + (Unit.custom ? GenerateValue(Unit.exception, data) : GenerateValue(Unit.message, data)) + MakeClickableForThisUnit(";") + "\n";
        }
    }
}