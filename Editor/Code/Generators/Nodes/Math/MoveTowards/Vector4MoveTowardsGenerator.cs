using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community 
{
    [NodeGenerator(typeof(Vector4MoveTowards))]
    public class Vector4Generator : BaseMoveTowardsGenerator<Vector4>
    {
        public Vector4Generator(Unit unit) : base(unit) { }

        protected override Type MoveTowardsClass => typeof(Vector4);
    } 
}
