
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(ScalarDivide))]
    public class ScalarDivideGenerator : DivideGenerator<float>
    {
        public ScalarDivideGenerator(Unit unit) : base(unit) { }
    }
}
