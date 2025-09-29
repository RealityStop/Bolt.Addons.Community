using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector4Distance))]
    public class Vector4DistanceGenerator : BaseDistanceGenerator<Vector4>
    {
        public Vector4DistanceGenerator(Unit unit) : base(unit) { }
    }
}