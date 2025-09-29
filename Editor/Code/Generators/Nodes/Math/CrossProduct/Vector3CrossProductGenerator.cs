using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Vector3CrossProduct))]
    public class Vector3CrossProductGenerator : NodeGenerator<Vector3CrossProduct>
    {
        public Vector3CrossProductGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            var a = GenerateValue(Unit.a, data);
            var b = GenerateValue(Unit.b, data);
            return CodeBuilder.StaticCall(Unit, typeof(Vector3), "Cross", true, a, b);
        }
    }
}