using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(CreateStruct))]
    public class CreateStructGenerator : NodeGenerator<CreateStruct>
    {
        public CreateStructGenerator(Unit unit) : base(unit) { }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return MakeClickableForThisUnit("new ".ConstructHighlight() + Unit.type.As().CSharpName(false, true) + "()");
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            if (!Unit.output.hasValidConnection)
            {
                return MakeClickableForThisUnit("new ".ConstructHighlight() + Unit.type.As().CSharpName(false, true) + "();") + "\n";
            }
            else return "";
        }
    }
}