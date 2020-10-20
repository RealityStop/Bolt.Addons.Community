using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Bolt.Addons.Community.Fundamentals
{
    [UnitCategory("Variables")]
    [UnitShortTitle("Decrement Variable")]
    [RenamedFrom("Bolt.Addons.Community.Variables.Units.DecrementUnit")]
    [UnitTitle("Decrement")]
    public sealed class DecrementUnit : IncrementUnit
    {
        public DecrementUnit() : base() { }

        protected override float GetAmount(Flow flow)
        {
            return -1;
        }
    }
}