using Bolt.Addons.Community.Fundamentals.Units.logic;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Fundamentals.Editor.Editor
{
    [Descriptor(typeof(FuncUnit))]
    public sealed class FuncUnitDescriptor : DelegateUnitDescriptor<FuncUnit>
    {
        public FuncUnitDescriptor(FuncUnit target) : base(target)
        {
        }
        
        protected override EditorTexture DefaultIcon()
        {
            return PathUtil.Load("func", CommunityEditorPath.Fundamentals);
        }

        protected override string DefaultName()
        {
            return "Func";
        }

        protected override EditorTexture DefinedIcon()
        {
            return PathUtil.Load("func", CommunityEditorPath.Fundamentals);
        }

        protected override string DefinedName()
        {
            return "Func";
        }
    }
}