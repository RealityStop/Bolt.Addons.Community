using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(UnitMethodDeclaration))]
    public sealed class UnitMethodDeclarationDescriptor : MacroDescriptor<UnitMethodDeclaration, MacroDescription>
    {
        public UnitMethodDeclarationDescriptor(UnitMethodDeclaration target) : base(target)
        {
        }
    }
}
