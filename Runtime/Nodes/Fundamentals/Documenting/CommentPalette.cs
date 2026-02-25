using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.Units.Documenting.CommentPalette")]
    public sealed class CommentPalette : ScriptableObject
    {
        public bool greyScale = false;
        public float colorSpread = 2.4f;
        public float colorHeight = 0.8f;
        public float colorOffset = 0.44f;
    }
}