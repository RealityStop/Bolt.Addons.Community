
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public abstract class PerSecondGenerator<T> : NodeGenerator<PerSecond<T>>
    {
        public PerSecondGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return GenerateValue(Unit.input, data) + MakeClickableForThisUnit(" * " + "Time".TypeHighlight() + "." + "deltaTime".VariableHighlight());
        }
    }
}
