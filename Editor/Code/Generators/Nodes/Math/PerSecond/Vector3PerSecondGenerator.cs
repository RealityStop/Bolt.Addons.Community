using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Vector3PerSecond))]
    public class Vector3PerSecondGenerator : PerSecondGenerator<Vector3>
    {
        public Vector3PerSecondGenerator(Unit unit) : base(unit) { }
    }
}
