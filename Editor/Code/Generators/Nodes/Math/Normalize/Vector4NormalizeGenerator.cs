using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector4Normalize))]
    public class Vector4NormalizeGenerator : BaseNormalizeGenerator<Vector4>
    {
        public Vector4NormalizeGenerator(Unit unit) : base(unit) { }
    }
}
