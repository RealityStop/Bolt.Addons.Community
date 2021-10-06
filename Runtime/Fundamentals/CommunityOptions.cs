using UnityEngine.Scripting;

[assembly:Preserve]

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.CommunityOptions")]
    public class CommunityOptions
    {
        public virtual bool DefinedEvent_ForceOptimizedInEditor { get; } = false;
        public virtual bool DefinedEvent_RestrictEventTypes { get; } = false;


        public virtual bool SilenceLogMessages { get; } = true;
    }
}
