using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Vector2MoveTowards))]
    public class Vector2Generator : BaseMoveTowardsGenerator<Vector2>
    {
        public Vector2Generator(Unit unit) : base(unit) { }

        protected override Type MoveTowardsClass => typeof(Vector2);
    } 
}
