using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community 
{
    [NodeGenerator(typeof(Vector3MoveTowards))]
    public class Vector3Generator : BaseMoveTowardsGenerator<Vector3>
    {
        public Vector3Generator(Unit unit) : base(unit) { }

        protected override Type MoveTowardsClass => typeof(Vector3);
    } 
}
