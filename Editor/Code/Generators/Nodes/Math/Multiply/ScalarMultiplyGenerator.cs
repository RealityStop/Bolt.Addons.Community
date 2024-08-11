
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(ScalarMultiply))]
    public class ScalarMultiplyGenerator : MultiplyGenerator<float>
    {
        public ScalarMultiplyGenerator(Unit unit) : base(unit) { }
    }
}
