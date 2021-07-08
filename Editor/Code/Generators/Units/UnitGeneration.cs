using Unity.VisualScripting;

namespace Bolt.Addons.Community.Generation
{
    public static class UnitGeneration
    {
        public static string GenerateValue<T>(this T unit, ValueInput input) where T : Unit
        {
            return UnitGenerator<T>.GetSingleDecorator(unit, unit).GenerateValue(input);
        }

        public static string GenerateValue<T>(this T unit, ValueOutput output) where T : Unit
        {
            return UnitGenerator<T>.GetSingleDecorator(unit, unit).GenerateValue(output);
        }

        public static string GenerateControl<T>(this T unit, ControlInput input, ControlGenerationData data, int indent) where T : Unit
        {
            return UnitGenerator<T>.GetSingleDecorator(unit, unit).GenerateControl(input, data, indent);
        }
    }
}