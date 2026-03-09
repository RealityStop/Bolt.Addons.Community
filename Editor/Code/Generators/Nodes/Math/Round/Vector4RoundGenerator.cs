using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Vector4Round))]
    public class Vector4RoundGenerator : BaseRoundGenerator<Vector4, Vector4>
    {
        public Vector4RoundGenerator(Unit unit) : base(unit) { }
    }
}