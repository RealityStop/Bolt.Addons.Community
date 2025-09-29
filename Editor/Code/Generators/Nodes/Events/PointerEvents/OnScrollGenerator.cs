using UnityEngine.EventSystems;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnScroll))]
    public class OnScrollGenerator : PointerEventUnitGenerator<OnScroll, IScrollHandler>
    {
        public OnScrollGenerator(Unit unit) : base(unit) { }
    }
}