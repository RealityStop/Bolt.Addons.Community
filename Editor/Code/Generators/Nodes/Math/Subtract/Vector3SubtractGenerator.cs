
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector3Subtract))]
    public class Vector3SubtractGenerator : SubtractGenerator<Vector3>
    {
        public Vector3SubtractGenerator(Unit unit) : base(unit) { }
    }
}
