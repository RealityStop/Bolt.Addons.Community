using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Bolt.Addons.Community.Fundamentals.Units.Documenting
{
    [UnitCategory("Community\\Documentation")]
    public class SomeValue : Unit
    {
        [Inspectable]
        public bool IsInteger;

        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput output { get; private set; }

        protected override void Definition()
        {
            if (IsInteger)
                output = ValueOutput<int>(nameof(output), (flow)=> UnityEngine.Random.Range(0,1));
            else
                output = ValueOutput<float>(nameof(output), (flow) => UnityEngine.Random.Range(0.0f, 1.0f));
        }
    }
}