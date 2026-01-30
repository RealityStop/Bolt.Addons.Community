using System;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(RandomStringNode))]
    public class RandomStringNodeGenerator : NodeGenerator<RandomStringNode>
    {
        public RandomStringNodeGenerator(Unit unit) : base(unit) { }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.CallCSharpUtilityMethod("RandomString");
        }
    }
}