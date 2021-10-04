using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(PropertyGetterMacro))]
    public sealed class PropertyGetterDescriptor : MacroDescriptor<PropertyGetterMacro, MacroDescription>
    {
        public PropertyGetterDescriptor(PropertyGetterMacro target) : base(target)
        {
        }
    }
}
