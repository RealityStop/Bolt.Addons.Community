using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public abstract class BaseDotProductGenerator<TVector> : NodeGenerator<DotProduct<TVector>>
    {
        public BaseDotProductGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            data.SetExpectedType(typeof(TVector));
            var a = GenerateValue(Unit.a, data);
            var b = GenerateValue(Unit.b, data);
            data.RemoveExpectedType();
            return CodeBuilder.StaticCall(Unit, typeof(TVector), "Dot", true, a, b);
        }
    }
}