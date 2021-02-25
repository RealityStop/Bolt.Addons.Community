using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


namespace Bolt.Addons.Community.Utility
{
    [RenamedFrom("Bolt.Community.Addons.Utility.UnitButtonAttribute")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class UnitButtonAttribute : Attribute
    {
        public string action;

        public UnitButtonAttribute(string action)
        {
            this.action = action;
        }
    }
}