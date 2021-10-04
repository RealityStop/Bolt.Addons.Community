using System.Collections;
using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMCollections
    {
        public static HUMCollections.Data.Move Move(this IList list, int itemIndex)
        {
            return new Data.Move(list);
        }
    }
}
