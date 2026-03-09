using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Vector2Distance))]
    public class Vector2DistanceGenerator : BaseDistanceGenerator<Vector2>
    {
        public Vector2DistanceGenerator(Unit unit) : base(unit) { }
    }
}