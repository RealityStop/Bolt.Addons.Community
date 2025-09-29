using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{  
    public abstract class InterfaceNodeGenerator : MethodNodeGenerator
    {
        public abstract List<Type> InterfaceTypes { get; }
        public InterfaceNodeGenerator(Unit unit) : base(unit) { }
    }
}