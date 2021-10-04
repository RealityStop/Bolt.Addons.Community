using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMAnimation_Children
    {
        public static ImmediateAnimation Animation(this HUMAnimation.Data.Create animation, bool unscaledTime = false)
        {
            var _animation = new ImmediateAnimation(animation.animator, unscaledTime);
            return _animation;
        }
    }
}