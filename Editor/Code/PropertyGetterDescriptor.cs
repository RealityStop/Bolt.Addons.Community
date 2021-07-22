using Unity.VisualScripting;

namespace Bolt.Addons.Community.Code.Editor
{
    [Descriptor(typeof(PropertyGetterMacro))]
    public sealed class PropertyGetterDescriptor : MacroDescriptor<PropertyGetterMacro, MacroDescription>
    {
        public PropertyGetterDescriptor(PropertyGetterMacro target) : base(target)
        {
        }
    }
}
