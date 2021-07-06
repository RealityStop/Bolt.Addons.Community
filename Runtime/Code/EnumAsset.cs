using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Code
{
    [CreateAssetMenu(menuName = "Visual Scripting/Community/Code/Enum")]
    public class EnumAsset : CodeAsset
    {
        [Inspectable]
        public bool useIndex;
        public bool itemsOpen;
        [Inspectable]
        public List<EnumItem> items = new List<EnumItem>();
    }
}
