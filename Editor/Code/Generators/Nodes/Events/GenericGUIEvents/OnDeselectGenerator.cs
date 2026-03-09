using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnDeselect))]
    public class OnDeselectGenerator : GenericGuiEventUnitGenerator<OnDeselect, IDeselectHandler>
    {
        public OnDeselectGenerator(Unit unit) : base(unit) { }
    }
}