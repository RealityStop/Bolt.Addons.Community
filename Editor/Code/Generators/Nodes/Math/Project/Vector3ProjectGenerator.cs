using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector3Project))]
    public class Vector3ProjectGenerator : BaseProjectGenerator<Vector3>
    {
        public Vector3ProjectGenerator(Unit unit) : base(unit) { }
    }
}