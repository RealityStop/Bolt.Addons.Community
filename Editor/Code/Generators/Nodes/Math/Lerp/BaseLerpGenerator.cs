using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    public abstract class BaseLerpGenerator<T> : NodeGenerator<Lerp<T>>
    {
        protected abstract Type LerpClass { get; }
        public BaseLerpGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            data.SetExpectedType(typeof(T));
            var a = GenerateValue(Unit.a, data);
            var b = GenerateValue(Unit.b, data);
            data.RemoveExpectedType();

            data.SetExpectedType(typeof(float));
            var t = GenerateValue(Unit.t, data);
            data.RemoveExpectedType();

            return CodeBuilder.StaticCall(Unit, LerpClass, "Lerp", true, a, b, t);
        }
    }
}