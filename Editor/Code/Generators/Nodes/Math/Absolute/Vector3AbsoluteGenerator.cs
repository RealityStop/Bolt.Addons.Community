using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector3Absolute))]
    public class Vector3AbsoluteGenerator : BaseAbsoluteGenerator<Vector3>
    {
        public Vector3AbsoluteGenerator(Unit unit) : base(unit) { }
    }
}