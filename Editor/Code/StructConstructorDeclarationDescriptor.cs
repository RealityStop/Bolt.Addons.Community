using Unity.VisualScripting;

namespace Bolt.Addons.Community.Code.Editor
{
    [Descriptor(typeof(StructConstructorDeclaration))]
    public sealed class StructConstructorDeclarationDescriptor : MacroDescriptor<StructConstructorDeclaration, MacroDescription>
    {
        public StructConstructorDeclarationDescriptor(StructConstructorDeclaration target) : base(target)
        {
        }
    }
}
