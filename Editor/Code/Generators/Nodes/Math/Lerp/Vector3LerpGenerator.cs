using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector3Lerp))]
    public class Vector3LerpGenerator : BaseLerpGenerator<Vector3>
    {
        public Vector3LerpGenerator(Unit unit) : base(unit) { }

        protected override Type LerpClass => typeof(Vector3);
    }
}