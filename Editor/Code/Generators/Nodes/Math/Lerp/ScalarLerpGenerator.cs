using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(ScalarLerp))]
    public class ScalarLerpGenerator : BaseLerpGenerator<float>
    {
        public ScalarLerpGenerator(Unit unit) : base(unit) { }

        protected override Type LerpClass => typeof(Mathf);
    }
}