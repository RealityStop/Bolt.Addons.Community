using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(ScalarMinimum))]
    public class ScalarMinimumGenerator : BaseMinimumGenerator<float>
    {
        public ScalarMinimumGenerator(Unit unit) : base(unit) { }
    }
}