
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector3Divide))]
    public class Vector3DivideGenerator : DivideGenerator<Vector3>
    {
        public Vector3DivideGenerator(Unit unit) : base(unit) { }
    }
}
