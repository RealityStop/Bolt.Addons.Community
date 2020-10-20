using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Scripting;


[assembly:Preserve]

namespace Bolt.Addons.Community
{
    public class CommunityOptions
    {
        public virtual bool DefinedEvent_ForceOptimizedInEditor { get; } = false;
        public virtual bool DefinedEvent_RestrictEventTypes { get; } = false;


        public virtual bool SilenceLogMessages { get; } = true;
    }
}
