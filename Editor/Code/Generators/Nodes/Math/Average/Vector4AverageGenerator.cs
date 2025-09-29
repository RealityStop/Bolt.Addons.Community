using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector4Average))]
    public class Vector4AverageGenerator : BaseAverageGenerator<Vector4>
    {
        public Vector4AverageGenerator(Unit unit) : base(unit) { }
    }
}