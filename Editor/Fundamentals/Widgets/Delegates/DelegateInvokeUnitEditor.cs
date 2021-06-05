using Unity.VisualScripting;
using Bolt.Addons.Community.Fundamentals.Units.logic;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Reflection;
using Bolt.Addons.Community.Utility;

namespace Bolt.Addons.Community.Fundamentals.Units.Utility.Editor
{
    public abstract class DelegateInvokeUnitEditor<TDelegate> : DelegateUnitEditor<TDelegate> where TDelegate : IDelegate
    {
        public DelegateInvokeUnitEditor(Metadata metadata) : base(metadata)
        {
        }
    }
}