namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(FunctionNode))]
    public sealed class FunctionNodeDescriptor : UnitDescriptor<FunctionNode>
    {
        public FunctionNodeDescriptor(FunctionNode target) : base(target)
        {
        }

        protected override string DefinedShortTitle()
        {
            if (target.functionType == FunctionType.Method && target.methodDeclaration != null) return string.IsNullOrEmpty(target.methodDeclaration.methodName) ? target.methodDeclaration.name : target.methodDeclaration.methodName;
            if (target.functionType == FunctionType.Getter && target.fieldDeclaration != null && !string.IsNullOrEmpty(target.fieldDeclaration.getter.name)) return target.fieldDeclaration.getter.name.Replace(" Getter", string.Empty);
            if (target.functionType == FunctionType.Setter && target.fieldDeclaration != null && !string.IsNullOrEmpty(target.fieldDeclaration.setter.name)) return target.fieldDeclaration.setter.name.Replace(" Setter", string.Empty);
            if (target.functionType == FunctionType.Constructor) return "Constructor";
            return base.DefinedShortTitle();
        }

        protected override string DefinedSubtitle()
        {
            if (target.functionType == FunctionType.Method && target.methodDeclaration != null) return "Entry";
            if (target.functionType == FunctionType.Getter && target.fieldDeclaration != null) return "Get";
            if (target.functionType == FunctionType.Setter && target.fieldDeclaration != null) return "Set";
            if (target.functionType == FunctionType.Constructor && target.constructorDeclaration != null)
            {
                if (target.constructorDeclaration.structAsset != null) return target.constructorDeclaration.structAsset.title;
                return target.constructorDeclaration.classAsset?.title;
            }
            return base.DefinedSubtitle();
        }

        protected override EditorTexture DefinedIcon()
        {
            if (target.functionType == FunctionType.Constructor) return PathUtil.Load("constructor_32", CommunityEditorPath.Code);
            if (target.functionType == FunctionType.Method) return PathUtil.Load("flow_32", CommunityEditorPath.Code);
            if (target.functionType == FunctionType.Getter || target.functionType == FunctionType.Setter) return PathUtil.Load("variables_32", CommunityEditorPath.Code);
            return PathUtil.Load("flow_32", CommunityEditorPath.Code);
        }

        protected override EditorTexture DefaultIcon()
        {
            return PathUtil.Load("flow_32", CommunityEditorPath.Code);
        }
    }
}