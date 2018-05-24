using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Bolt.Addons.Community.Variables.Units
{
    [UnitCategory("Variables")]
    [UnitShortTitle("Decrement Variable")]
    [UnitTitle("Decrement")]
    public sealed class DecrementUnit : IncrementUnit
    {
        public DecrementUnit() : base() { }

        protected override float GetAmount()
        {
            return -1;
        }
    }
}