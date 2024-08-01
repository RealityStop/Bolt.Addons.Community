using Unity.VisualScripting;
using System;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(ScalarMultiply))]
    public class ScalarMultiplyGenerator : NodeGenerator<ScalarMultiply>
    {
        public ScalarMultiplyGenerator(Unit unit) : base(unit)
        {
        }

        public override string GenerateValue(ValueOutput output)
        {
            return base.GenerateValue(this.Unit.a) + " * " + base.GenerateValue(this.Unit.b);
        }
    }
}