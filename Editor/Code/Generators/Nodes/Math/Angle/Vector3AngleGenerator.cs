using UnityEngine;

namespace Unity.VisualScripting.Community 
{
    [NodeGenerator(typeof(Vector3Angle))]
    public class Vector3AngleGenerator : BaseAngleGenerator<Vector3>
    {
        public Vector3AngleGenerator(Unit unit) : base(unit) { }
    } 
}
