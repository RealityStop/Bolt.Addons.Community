
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector4Subtract))]
    public class Vector4SubtractGenerator : SubtractGenerator<Vector4>
    {
        public Vector4SubtractGenerator(Unit unit) : base(unit) { }
    }
}
