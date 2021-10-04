using Unity.VisualScripting.Community.Utility;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(FuncInvokeNode))]
    public sealed class FuncInvokeNodeDescriptor : DelegateInvokeNodeDesriptor<FuncInvokeNode, IFunc>
    {
        protected override string Prefix => "Invoke";
        protected override string FallbackName => "Func";

        public FuncInvokeNodeDescriptor(FuncInvokeNode target) : base(target)
        {
        }

        protected override EditorTexture DefaultIcon()
        {
            return PathUtil.Load("func_invoke", CommunityEditorPath.Fundamentals);
        }

        protected override EditorTexture DefinedIcon()
        {
            return PathUtil.Load("func_invoke", CommunityEditorPath.Fundamentals);
        }
    }
}