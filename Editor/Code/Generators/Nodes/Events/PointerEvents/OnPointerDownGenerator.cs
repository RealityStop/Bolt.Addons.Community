using UnityEngine.EventSystems;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnPointerDown))]
    public class OnPointerDownGenerator : PointerEventUnitGenerator<OnPointerDown, IPointerDownHandler>
    {
        public OnPointerDownGenerator(Unit unit) : base(unit) { }
    }
}