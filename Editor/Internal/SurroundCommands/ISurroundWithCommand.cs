using Unity.VisualScripting;
namespace Unity.VisualScripting.Community
{
    internal interface ISurroundWithCommand : ISurroundWithCommandBase
    {
        public Unit SurroundUnit { get; }
        public IUnitPort autoConnectPort { get; }
    }

    internal interface ISurroundWithCommandBase
    {
        public ControlOutput surroundSource { get; }
        public ControlOutput surroundExit { get; }
        public ControlInput unitEnterPort { get; }
    }
}