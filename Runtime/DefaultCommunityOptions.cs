using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Community;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class DefaultCommunityOptions : CommunityOptions
    {
        public override bool DefinedEvent_ForceOptimizedInEditor => false;

        public override bool DefinedEvent_RestrictEventTypes => true;

        public override bool SilenceLogMessages => true;
    }
}
