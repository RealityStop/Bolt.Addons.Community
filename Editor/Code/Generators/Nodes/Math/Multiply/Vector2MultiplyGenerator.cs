
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector2Multiply))]
    public class Vector2MultiplyGenerator : MultiplyGenerator<Vector2>
    {
        public Vector2MultiplyGenerator(Unit unit) : base(unit) { }
    }
}
