using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(CreateList))]
    public class CreateListGenerator : NodeGenerator<CreateList>
    {
        public CreateListGenerator(Unit unit) : base(unit)
        {
            NameSpaces = "Unity.VisualScripting";
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            List<string> result = new List<string>();
            foreach (ValueInput item in Unit.multiInputs)
            {
                result.Add(base.GenerateValue(item, data));
            }
            var Type = typeof(AotList);
            if(data.GetExpectedType() != null)
            {
                Type = data.GetExpectedType();
            }
            return MakeClickableForThisUnit("new ".ConstructHighlight() + Type.As().CSharpName(false, true) + (!Type.IsArray ? "()" : string.Empty) + " { ") + string.Join(MakeClickableForThisUnit(", "), result) + MakeClickableForThisUnit(" }");
        }
    }
}