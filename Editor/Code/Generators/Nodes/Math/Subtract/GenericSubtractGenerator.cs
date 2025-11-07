using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(GenericSubtract))]
    public class GenericSubtractGenerator : SubtractGenerator<object>
    {
        public GenericSubtractGenerator(Unit unit) : base(unit) { }
    }
}
