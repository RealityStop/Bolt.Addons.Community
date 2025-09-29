using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector4Absolute))]
    public class Vector4AbsoluteGenerator : BaseAbsoluteGenerator<Vector4>
    {
        public Vector4AbsoluteGenerator(Unit unit) : base(unit) { }
    }
}