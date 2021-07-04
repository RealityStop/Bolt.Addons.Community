

using Bolt.Addons.Libraries.CSharp;
using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Bolt.Addons.Community.Utility
{
    [Inspectable]
    [RenamedFrom("Bolt.Community.Addons.Utility.UnitButton")]
    public class UnitButton
    {
        public System.Action action;
    }
}