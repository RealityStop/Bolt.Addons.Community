using System;
using System.Linq;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("RandomStringNode")]
    [UnitTitle("Random String")]
    [UnitCategory("Community\\Utility\\string")]
    [TypeIcon(typeof(string))]
    public class RandomStringNode : Unit
    {
        [DoNotSerialize]
        public ValueOutput Output;

        protected override void Definition()
        {
            Output = ValueOutput<string>("Output", GetRandomString);
        }

        public string GetRandomString(Flow flow)
        {
            return CSharpUtility.RandomString();
        }
    }

}