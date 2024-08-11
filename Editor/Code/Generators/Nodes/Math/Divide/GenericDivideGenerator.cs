
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(GenericDivide))]
    public class GenericDivideGenerator : DivideGenerator<object>
    {
        public GenericDivideGenerator(Unit unit) : base(unit) { }
    }
}
