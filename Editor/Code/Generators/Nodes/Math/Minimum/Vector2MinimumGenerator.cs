using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector2Minimum))]
    public class Vector2MinimumGenerator : BaseMinimumGenerator<Vector2>
    {
        public Vector2MinimumGenerator(Unit unit) : base(unit) { }
    }
}