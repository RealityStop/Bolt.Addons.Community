using Bolt.Addons.Community.Fundamentals.Units.logic;
using Bolt.Addons.Community.Utility;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Fundamentals.Editor.Editor
{
    [Descriptor(typeof(UnbindFuncUnit))]
    public sealed class UnbindFuncUnitDescriptor : UnbindDelegateUnitDescriptor<UnbindFuncUnit, IFunc>
    {
        public UnbindFuncUnitDescriptor(UnbindFuncUnit target) : base(target)
        {
        }

        protected override EditorTexture DefaultIcon()
        {
            return PathUtil.Load("func_unbind", CommunityEditorPath.Fundamentals);
        }

        protected override string DefaultName()
        {
            return "Unbind";
        }

        protected override EditorTexture DefinedIcon()
        {
            return PathUtil.Load("func_unbind", CommunityEditorPath.Fundamentals);
        }

        protected override string DefinedName()
        {
            return "Unbind";
        }
    }
}