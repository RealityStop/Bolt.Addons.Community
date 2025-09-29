using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector4Lerp))]
    public class Vector4LerpGenerator : BaseLerpGenerator<Vector4>
    {
        public Vector4LerpGenerator(Unit unit) : base(unit) { }

        protected override Type LerpClass => typeof(Vector4);
    }
}