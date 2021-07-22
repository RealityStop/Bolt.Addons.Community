using Unity.VisualScripting;

namespace Bolt.Addons.Community.Code.Editor
{
    [Descriptor(typeof(StructMethodDeclaration))]
    public sealed class StructMethodDeclarationDescriptor : MacroDescriptor<StructMethodDeclaration, MacroDescription>
    {
        public StructMethodDeclarationDescriptor(StructMethodDeclaration target) : base(target)
        {
        }
    }
}
