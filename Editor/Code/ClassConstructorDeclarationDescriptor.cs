using Unity.VisualScripting;

namespace Bolt.Addons.Community.Code.Editor
{
    [Descriptor(typeof(ClassConstructorDeclaration))]
    public sealed class ClassConstructorDeclarationDescriptor : MacroDescriptor<ClassConstructorDeclaration, MacroDescription>
    {
        public ClassConstructorDeclarationDescriptor(ClassConstructorDeclaration target) : base(target)
        {
        }
    }
}
