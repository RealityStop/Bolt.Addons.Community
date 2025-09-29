using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnDrag))]
    public class OnDragGenerator : PointerEventUnitGenerator<OnDrag, IDragHandler>
    {
        public OnDragGenerator(Unit unit) : base(unit) { }
    }
}