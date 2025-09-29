using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector3Normalize))]
    public class Vector3NormalizeGenerator : BaseNormalizeGenerator<Vector3>
    {
        public Vector3NormalizeGenerator(Unit unit) : base(unit) { }
    }
}
