using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector4Maximum))]
    public class Vector4MaximumGenerator : BaseMaximumGenerator<Vector4>
    {
        public Vector4MaximumGenerator(Unit unit) : base(unit) { }
    }
}