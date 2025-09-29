using NUnit.Framework.Internal;

namespace Unity.VisualScripting.Community
{
    public abstract class SurroundCommand : ISurroundWithCommand
    {
        private Sequence _sequenceUnit = new Sequence() { outputCount = 2 };

        public abstract Unit SurroundUnit { get; }

        public abstract ControlOutput surroundSource { get; }

        public abstract ControlOutput surroundExit { get; }

        public abstract ControlInput unitEnterPort { get; }

        public abstract IUnitPort autoConnectPort { get; }

        public abstract string DisplayName { get; }
        public abstract bool SequenceExit { get; }

        public Sequence sequenceUnit
        {
            get
            {
                if (_sequenceUnit == null)
                {
                    _sequenceUnit = new Sequence() { outputCount = 2 };
                }
                _sequenceUnit.EnsureDefined();
                return _sequenceUnit;
            }
        }
    }
}
