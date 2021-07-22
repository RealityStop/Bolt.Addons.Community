using Unity.VisualScripting;

namespace Bolt.Addons.Community.Code.Editor
{
    [Descriptor(typeof(PropertySetterMacro))]
    public sealed class PropertySetterDescriptor : MacroDescriptor<PropertySetterMacro, MacroDescription>
    {
        public PropertySetterDescriptor(PropertySetterMacro target) : base(target)
        {
        }
    }
}
