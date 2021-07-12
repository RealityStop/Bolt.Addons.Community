using Bolt.Addons.Community.Code;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Generation
{
    [UnitGenerator(typeof(FunctionUnit))]
    public sealed class FunctionUnitGenerator : UnitGenerator<FunctionUnit>
    {
        public FunctionUnitGenerator(FunctionUnit unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var destination = Unit.invoke.connection?.destination;
            if (destination == null) return string.Empty;
            return UnitGenerator.GetSingleDecorator(destination.unit as Unit, destination.unit as Unit).GenerateControl(destination, data, indent);
        }
    }
}