using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(RandomStringNode))]
    public class RandomStringNodeGenerator : NodeGenerator<RandomStringNode>
    {
        public RandomStringNodeGenerator(Unit unit) : base(unit) { }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return CodeBuilder.CallCSharpUtilityMethod(Unit, MakeClickableForThisUnit("RandomString"));
        }
    }
}