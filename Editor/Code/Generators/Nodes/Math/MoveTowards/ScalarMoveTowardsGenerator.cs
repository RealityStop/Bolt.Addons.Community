using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community 
{
    [NodeGenerator(typeof(ScalarMoveTowards))]
    public class ScalarMoveTowardsGenerator : BaseMoveTowardsGenerator<float>
    {
        public ScalarMoveTowardsGenerator(Unit unit) : base(unit) { }

        protected override Type MoveTowardsClass => typeof(Mathf);
    } 
}
