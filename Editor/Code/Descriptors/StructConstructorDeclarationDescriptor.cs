using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(StructConstructorDeclaration))]
    public sealed class StructConstructorDeclarationDescriptor : MacroDescriptor<StructConstructorDeclaration, MacroDescription>
    {
        public StructConstructorDeclarationDescriptor(StructConstructorDeclaration target) : base(target)
        {
        }
    }
}
