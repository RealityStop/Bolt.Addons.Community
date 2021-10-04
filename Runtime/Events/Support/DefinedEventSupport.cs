using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.DefinedEvents.Support.DefinedEventSupport")]
    static class DefinedEventSupport
    {
        internal static bool IsOptimized()
        {
            if (Application.isEditor && !CommunityOptionFetcher.DefinedEvent_ForceOptimizedInEditor)
                return false;
            return true;
        }
    }
}