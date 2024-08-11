using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(CreateList))]
    public class CreateListGenerator : NodeGenerator<CreateList>
    {
        public CreateListGenerator(Unit unit) : base(unit)
        {
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            List<string> result = new List<string>();
            foreach (ValueInput item in this.Unit.multiInputs)
            {
                result.Add(base.GenerateValue(item, data));
            }
            return "new".ConstructHighlight() + " AotList".TypeHighlight() + "() { " + string.Join(", ", result) + " }";
        }
    }
}