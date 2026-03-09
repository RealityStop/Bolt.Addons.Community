
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Vector4Modulo))]
    public class Vector4ModuloGenerator : ModuloGenerator<Vector4>
    {
        public Vector4ModuloGenerator(Unit unit) : base(unit) { }
    }
}
