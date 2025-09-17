using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnSelect))]
    public class OnSelectGenerator : GenericGuiEventUnitGenerator<OnSelect, ISelectHandler>
    {
        public OnSelectGenerator(Unit unit) : base(unit) { }
    }
}