
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(ScalarModulo))]
    public class ScalarModuloGenerator : ModuloGenerator<float>
    {
        public ScalarModuloGenerator(Unit unit) : base(unit) { }
    }
}
