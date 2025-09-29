using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector2PerSecond))]
    public class Vector2PerSecondGenerator : PerSecondGenerator<Vector2>
    {
        public Vector2PerSecondGenerator(Unit unit) : base(unit) { }
    }
}
