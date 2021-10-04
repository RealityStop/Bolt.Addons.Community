using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [CreateAssetMenu(menuName = "Visual Scripting/Community/Code/Enum")]
    [RenamedFrom("Bolt.Addons.Community.Code.EnumAsset")]
    public class EnumAsset : CodeAsset
    {
        [Inspectable]
        public bool useIndex;
        public bool itemsOpen;
        [Inspectable]
        public List<EnumItem> items = new List<EnumItem>();
    }
}
