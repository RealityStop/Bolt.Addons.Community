using Unity.VisualScripting;
using System;
using UnityEngine;

namespace Unity.VisualScripting.Community.Utility
{
    [IncludeInSettings(true)]
    [Serializable][Inspectable]
    [RenamedFrom("Bolt.Addons.Community.Utility.Editor.CustomVariable")]
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