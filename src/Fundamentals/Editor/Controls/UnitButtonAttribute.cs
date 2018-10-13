using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bolt.Community.Addons.Fundamentals.Editor.Controls
{
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