using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Vector3Average))]
    public class Vector3AverageGenerator : BaseAverageGenerator<Vector3>
    {
        public Vector3AverageGenerator(Unit unit) : base(unit) { }
    }
}