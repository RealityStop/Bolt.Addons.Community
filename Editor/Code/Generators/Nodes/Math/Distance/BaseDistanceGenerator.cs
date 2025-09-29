using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public abstract class BaseDistanceGenerator<T> : NodeGenerator<Distance<T>>
    {
        public BaseDistanceGenerator(Unit unit) : base(unit) { NameSpaces = "UnityEngine"; }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            data.SetExpectedType(typeof(T));
            string a = GenerateValue(Unit.a, data);
            string b = GenerateValue(Unit.b, data);
            data.RemoveExpectedType();
            return typeof(T).As().CSharpName(false, true) + MakeClickableForThisUnit(".Distance(") + a + MakeClickableForThisUnit(", ") + b + MakeClickableForThisUnit(")");
        }
    }

}
