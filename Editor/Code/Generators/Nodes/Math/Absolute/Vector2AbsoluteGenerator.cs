using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector2Absolute))]
    public class Vector2AbsoluteGenerator : BaseAbsoluteGenerator<Vector2>
    {
        public Vector2AbsoluteGenerator(Unit unit) : base(unit) { }
    }
}