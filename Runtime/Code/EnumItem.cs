using System;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    [Inspectable]
    [RenamedFrom("Bolt.Addons.Community.Code.EnumItem")]
    public class EnumItem
    {
        [Inspectable]
        public string name;
        [Inspectable]
        public int index;
    }
}