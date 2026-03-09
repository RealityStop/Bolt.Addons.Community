
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Vector3Multiply))]
    public class Vector3MultiplyGenerator : MultiplyGenerator<Vector3>
    {
        public Vector3MultiplyGenerator(Unit unit) : base(unit) { }
    }
}
