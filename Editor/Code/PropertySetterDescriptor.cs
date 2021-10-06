using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(PropertySetterMacro))]
    public sealed class PropertySetterDescriptor : MacroDescriptor<PropertySetterMacro, MacroDescription>
    {
        public PropertySetterDescriptor(PropertySetterMacro target) : base(target)
        {
        }
    }
}
