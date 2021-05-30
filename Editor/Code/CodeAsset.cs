using UnityEngine;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Code.Editor
{
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
    }
}
