using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector2Subtract))]
    public class Vector2SubtractGenerator : SubtractGenerator<Vector2>
    {
        public Vector2SubtractGenerator(Unit unit) : base(unit) { }
    }
}
