using UnityEngine;

namespace Unity.VisualScripting.Community 
{
    [NodeGenerator(typeof(Vector2Angle))]
    public class Vector2AngleGenerator : BaseAngleGenerator<Vector2>
    {
        public Vector2AngleGenerator(Unit unit) : base(unit) { }
    } 
}
