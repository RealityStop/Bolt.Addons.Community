using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(ClassMethodDeclaration))]
    public sealed class ClassMethodDeclarationDescriptor : MacroDescriptor<ClassMethodDeclaration, MacroDescription>
    {
        public ClassMethodDeclarationDescriptor(ClassMethodDeclaration target) : base(target)
        {
        }
    }
}
