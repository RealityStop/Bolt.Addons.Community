using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector3Round))]
    public class Vector3RoundGenerator : BaseRoundGenerator<Vector3, Vector3>
    {
        public Vector3RoundGenerator(Unit unit) : base(unit) { }
    }
}