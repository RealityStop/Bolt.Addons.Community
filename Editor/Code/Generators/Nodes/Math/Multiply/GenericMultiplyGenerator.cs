
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(GenericMultiply))]
    public class GenericMultiplyGenerator : MultiplyGenerator<object>
    {
        public GenericMultiplyGenerator(Unit unit) : base(unit) { }
    }
}
