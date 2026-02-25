using System.Collections.Generic;

namespace Unity.VisualScripting.Community
{
    public class EventUnitDescriptor<TEvent> : UnitDescriptor<TEvent>
        where TEvent : class, IEventUnit
    {
        public EventUnitDescriptor(TEvent @event) : base(@event) { }

        protected override IEnumerable<EditorTexture> DefinedIcons()
        {
            if (unit.coroutine)
            {
                yield return BoltFlow.Icons.coroutine;
            }
        }
    }
}