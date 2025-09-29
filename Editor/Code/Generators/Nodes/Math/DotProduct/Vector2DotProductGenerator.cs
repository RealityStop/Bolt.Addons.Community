using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector2DotProduct))]
    public class Vector2DotProductGenerator : BaseDotProductGenerator<Vector2>
    {
        public Vector2DotProductGenerator(Unit unit) : base(unit) { }
    }
}