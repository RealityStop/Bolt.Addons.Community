using Unity.VisualScripting;

namespace Bolt.Addons.Community.Code.Editor
{
    [Descriptor(typeof(ClassMethodDeclaration))]
    public sealed class ClassMethodDeclarationDescriptor : MacroDescriptor<ClassMethodDeclaration, MacroDescription>
    {
        public ClassMethodDeclarationDescriptor(ClassMethodDeclaration target) : base(target)
        {
        }
    }
}
