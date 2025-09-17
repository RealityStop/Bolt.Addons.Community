using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector4DotProduct))]
    public class Vector4DotProductGenerator : BaseDotProductGenerator<Vector4>
    {
        public Vector4DotProductGenerator(Unit unit) : base(unit) { }
    }
}