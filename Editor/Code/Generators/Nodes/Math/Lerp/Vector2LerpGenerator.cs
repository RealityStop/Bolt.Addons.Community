using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector2Lerp))]
    public class Vector2LerpGenerator : BaseLerpGenerator<Vector2>
    {
        public Vector2LerpGenerator(Unit unit) : base(unit) { }

        protected override Type LerpClass => typeof(Vector2);
    }

}