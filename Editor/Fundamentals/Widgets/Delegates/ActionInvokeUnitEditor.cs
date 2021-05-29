using Ludiq;
using Bolt.Addons.Community.Fundamentals.Units.logic;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Reflection;
using Bolt.Addons.Community.Utility;

namespace Bolt.Addons.Community.Fundamentals.Units.Utility.Editor
{
    [Editor(typeof(ActionInvokeUnit))]
    public sealed class ActionInvokeUnitEditor : DelegateInvokeUnitEditor<IAction>
    {
        public ActionInvokeUnitEditor(Metadata metadata) : base(metadata)
        {
        }

        protected override string DefaultName => "Action";
    }
}