﻿using System.Collections;
using UnityEngine;

namespace Bolt.Addons.Integrations.Continuum.Humility
{
    public static partial class HUMCollections
    {
        public static HUMCollections.Data.Move Move(this IList list, int itemIndex)
        {
            return new Data.Move(list);
        }
    }
}
