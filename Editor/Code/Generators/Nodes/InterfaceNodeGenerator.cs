using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community 
{
    public abstract class InterfaceNodeGenerator : MethodNodeGenerator
    {
        protected InterfaceNodeGenerator(Unit unit) : base(unit)
        {
        }

        public abstract IEnumerable<Type> InterfaceTypes { get; }
    }
}
