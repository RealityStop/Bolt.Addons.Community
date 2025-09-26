using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [SpecialUnit]
    [TypeIcon(typeof(ScalarSum))]
    public class FuzzyExpression : Unit
    {
        public List<string> tokens = new List<string>();

        protected override void Definition()
        {
        }
    }
}
