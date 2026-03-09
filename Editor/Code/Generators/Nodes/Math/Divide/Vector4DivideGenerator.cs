
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Vector4Divide))]
    public class Vector4DivideGenerator : DivideGenerator<Vector4>
    {
        public Vector4DivideGenerator(Unit unit) : base(unit) { }
    }
}
