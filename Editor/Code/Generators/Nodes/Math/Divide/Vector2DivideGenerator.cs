
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector2Divide))]
    public class Vector2DivideGenerator : DivideGenerator<Vector2>
    {
        public Vector2DivideGenerator(Unit unit) : base(unit) { }
    }
}
