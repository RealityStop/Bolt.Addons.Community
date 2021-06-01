using UnityEngine;
using Unity.VisualScripting;
using System;

namespace Bolt.Addons.Community.Code.Editor
{
    [Serializable]
    public abstract class CodeAsset : ScriptableObject
    {
        [Inspectable]
        [InspectorWide]
        public string title;
        [Inspectable]
        [InspectorWide]
        public string category;
        public bool optionsOpened;
        public bool preview;
        public string lastCompiledName;
    }
}
