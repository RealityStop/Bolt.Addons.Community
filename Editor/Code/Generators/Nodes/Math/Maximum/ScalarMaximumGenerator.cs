using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(ScalarMaximum))]
    public class ScalarMaximumGenerator : BaseMaximumGenerator<float>
    {
        public ScalarMaximumGenerator(Unit unit) : base(unit) { }
    }
}