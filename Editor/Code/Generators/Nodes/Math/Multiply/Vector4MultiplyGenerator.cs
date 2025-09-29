
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector4Multiply))]
    public class Vector4MultiplyGenerator : MultiplyGenerator<Vector4>
    {
        public Vector4MultiplyGenerator(Unit unit) : base(unit) { }
    }
}
