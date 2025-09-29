using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector3DotProduct))]
    public class Vector3DotProductGenerator : BaseDotProductGenerator<Vector3>
    {
        public Vector3DotProductGenerator(Unit unit) : base(unit) { }
    }
}