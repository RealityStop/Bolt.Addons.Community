using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector3Maximum))]
    public class Vector3MaximumGenerator : BaseMaximumGenerator<Vector3>
    {
        public Vector3MaximumGenerator(Unit unit) : base(unit) { }
    }
}