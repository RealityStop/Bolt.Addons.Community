using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(StructMethodDeclaration))]
    public sealed class StructMethodDeclarationDescriptor : MacroDescriptor<StructMethodDeclaration, MacroDescription>
    {
        public StructMethodDeclarationDescriptor(StructMethodDeclaration target) : base(target)
        {
        }
    }
}
