using UnityEngine.EventSystems;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnCancel))]
    public class OnCancelGenerator : GenericGuiEventUnitGenerator<OnCancel, ICancelHandler>
    {
        public OnCancelGenerator(Unit unit) : base(unit) { }
    }
}