using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector2Project))]
    public class Vector2ProjectGenerator : BaseProjectGenerator<Vector2>
    {
        public Vector2ProjectGenerator(Unit unit) : base(unit) { }
    }
}