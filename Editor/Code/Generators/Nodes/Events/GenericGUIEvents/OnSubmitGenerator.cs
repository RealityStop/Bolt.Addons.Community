using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnSubmit))]
    public class OnSubmitGenerator : GenericGuiEventUnitGenerator<OnSubmit, ISubmitHandler>
    {
        public OnSubmitGenerator(Unit unit) : base(unit) { }
    }
}