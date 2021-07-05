using System;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Code
{
    [Serializable]
    [Inspectable]
    public class EnumItem
    {
        [Inspectable]
        public string name;
        [Inspectable]
        public int index;
    }
}