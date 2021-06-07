using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bolt.Addons.Libraries.Humility
{
    public static partial class HUMAnimation
    {
        public static Data.Create Create(this Animator animator)
        {
            return new Data.Create(animator);
        }
    }
}