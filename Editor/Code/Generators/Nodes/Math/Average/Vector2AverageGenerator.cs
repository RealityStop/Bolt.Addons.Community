using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector2Average))]
    public class Vector2AverageGenerator : BaseAverageGenerator<Vector2>
    {
        public Vector2AverageGenerator(Unit unit) : base(unit) { }
    }
}
