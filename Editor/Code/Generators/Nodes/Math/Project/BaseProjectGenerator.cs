using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public abstract class BaseProjectGenerator<TVector> : NodeGenerator<Project<TVector>>
    {
        public BaseProjectGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            data.SetExpectedType(typeof(TVector));
            var a = GenerateValue(Unit.a, data);
            var b = GenerateValue(Unit.b, data);
            data.RemoveExpectedType();
            return CodeBuilder.CallCSharpUtilityMethod(Unit, "Project", a, b);
        }
    }
}