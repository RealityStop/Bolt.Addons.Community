using Unity.VisualScripting;
using System;
using UnityEngine;

namespace Bolt.Addons.Community.Utility.Editor
{
    [IncludeInSettings(true)]
    [Serializable][Inspectable]
    public sealed class CustomVariable
    {
        [Inspectable]
        [InspectorWide]
        [InspectorLabel(null)]
        [Serialize]
        [SerializeField]
        public string name;
        [Inspectable][Serialize][InspectorWide][InspectorLabel(null)]
        [SerializeReference]
        public object value;
    }
}