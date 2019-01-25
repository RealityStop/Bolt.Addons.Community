using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Bolt.Addons.Community.DefinedEvents.Support
{
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