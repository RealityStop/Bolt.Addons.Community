using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector2Maximum))]
    public class Vector2MaximumGenerator : BaseMaximumGenerator<Vector2>
    {
        public Vector2MaximumGenerator(Unit unit) : base(unit) { }
    }
}