using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Vector2Round))]
    public class Vector2RoundGenerator : BaseRoundGenerator<Vector2, Vector2>
    {
        public Vector2RoundGenerator(Unit unit) : base(unit) { }
    }
}