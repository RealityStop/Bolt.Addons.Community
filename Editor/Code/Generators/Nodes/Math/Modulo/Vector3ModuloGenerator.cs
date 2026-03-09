
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Vector3Modulo))]
    public class Vector3ModuloGenerator : ModuloGenerator<Vector3>
    {
        public Vector3ModuloGenerator(Unit unit) : base(unit) { }
    }
}
