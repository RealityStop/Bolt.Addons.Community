using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(ClassConstructorDeclaration))]
    public sealed class ClassConstructorDeclarationDescriptor : MacroDescriptor<ClassConstructorDeclaration, MacroDescription>
    {
        public ClassConstructorDeclarationDescriptor(ClassConstructorDeclaration target) : base(target)
        {
        }
    }
}
