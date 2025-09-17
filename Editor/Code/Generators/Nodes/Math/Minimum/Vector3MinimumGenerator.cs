using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector3Minimum))]
    public class Vector3MinimumGenerator : BaseMinimumGenerator<Vector3>
    {
        public Vector3MinimumGenerator(Unit unit) : base(unit) { }
    }
}