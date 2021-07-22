using Unity.VisualScripting;

namespace Bolt.Addons.Community.Code
{
    [Descriptor(typeof(FunctionUnit))]
    public sealed class FunctionUnitDescriptor : UnitDescriptor<FunctionUnit>
    {
        public FunctionUnitDescriptor(FunctionUnit target) : base(target)
        {
        }

        protected override string DefinedShortTitle()
        {
            if (target.functionType == FunctionType.Method && target.methodDeclaration != null) return string.IsNullOrEmpty(target.methodDeclaration.methodName) ? target.methodDeclaration.name : target.methodDeclaration.methodName;
            if (target.functionType == FunctionType.Getter && target.fieldDeclaration != null && !string.IsNullOrEmpty(target.fieldDeclaration.getter.name)) return target.fieldDeclaration.getter.name.Replace(" Getter", string.Empty);
            if (target.functionType == FunctionType.Setter && target.fieldDeclaration != null && !string.IsNullOrEmpty(target.fieldDeclaration.setter.name)) return target.fieldDeclaration.setter.name.Replace(" Setter", string.Empty);
            return base.DefinedShortTitle();
        }

        protected override string DefinedSubtitle()
        {
            if (target.functionType == FunctionType.Method && target.methodDeclaration != null) return "Entry";
            if (target.functionType == FunctionType.Getter && target.fieldDeclaration != null) return "Get";
            if (target.functionType == FunctionType.Setter && target.fieldDeclaration != null) return "Set";
            if (target.functionType == FunctionType.Constructor && target.constructorDeclaration != null) return "Constructor";
            return string.Empty;
        }

        protected override EditorTexture DefinedIcon()
        {
            return PathUtil.Load("flow_32", CommunityEditorPath.Code);
        }

        protected override EditorTexture DefaultIcon()
        {
            return PathUtil.Load("flow_32", CommunityEditorPath.Code);
        }
    }
}