
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Vector2Modulo))]
    public class Vector2ModuloGenerator : ModuloGenerator<Vector2>
    {
        public Vector2ModuloGenerator(Unit unit) : base(unit) { }
    }
}
