using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector4PerSecond))]
    public class Vector4PerSecondGenerator : PerSecondGenerator<Vector4>
    {
        public Vector4PerSecondGenerator(Unit unit) : base(unit) { }
    }
}
