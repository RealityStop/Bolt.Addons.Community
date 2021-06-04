using Bolt.Addons.Community.Fundamentals.Units.logic;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Fundamentals.Editor.Editor
{
    [Descriptor(typeof(FuncInvokeUnit))]
    public sealed class FuncInvokeUnitDescriptor : DelegateInvokeUnitDescriptor<FuncInvokeUnit>
    {
        protected override string Prefix => "Invoke";
        protected override string FallbackName => "Func";

        public FuncInvokeUnitDescriptor(FuncInvokeUnit target) : base(target)
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