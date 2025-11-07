
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(GenericModulo))]
    public class GenericModuloGenerator : ModuloGenerator<object>
    {
        public GenericModuloGenerator(Unit unit) : base(unit) { }
    }
}
