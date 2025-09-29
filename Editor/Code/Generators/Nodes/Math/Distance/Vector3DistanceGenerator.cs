using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector3Distance))]
    public class Vector3DistanceGenerator : BaseDistanceGenerator<Vector3>
    {
        public Vector3DistanceGenerator(Unit unit) : base(unit) { }
    }
}