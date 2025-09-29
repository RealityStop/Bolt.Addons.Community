using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(ScalarAbsolute))]
    public class ScalarAbsoluteGenerator : BaseAbsoluteGenerator<float>
    {
        public ScalarAbsoluteGenerator(Unit unit) : base(unit) { }
    }
}