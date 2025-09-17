using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector2Normalize))]
    public class Vector2NormalizeGenerator : BaseNormalizeGenerator<Vector2>
    {
        public Vector2NormalizeGenerator(Unit unit) : base(unit) { }
    }
}
