using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(ScalarRoot))]
    public class ScalarRootGenerator : NodeGenerator<ScalarRoot>
    {
        public ScalarRootGenerator(Unit unit) : base(unit) { }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            var radicand = GenerateValue(Unit.radicand, data);
            var degree = GenerateValue(Unit.degree, data);
            return CodeBuilder.CallCSharpUtilityMethod(Unit, "Root", radicand, degree);
        }
    }
}