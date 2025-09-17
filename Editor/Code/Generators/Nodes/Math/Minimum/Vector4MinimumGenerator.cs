using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector4Minimum))]
    public class Vector4MinimumGenerator : BaseMinimumGenerator<Vector4>
    {
        public Vector4MinimumGenerator(Unit unit) : base(unit) { }
    }
}