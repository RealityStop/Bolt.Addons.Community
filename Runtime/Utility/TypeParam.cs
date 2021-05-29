using System;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Utility
{
    [Serializable]
    public sealed class TypeParam
    {
        [Serialize]
        public Type type;
        [Serialize]
        public string name;
    }
}
