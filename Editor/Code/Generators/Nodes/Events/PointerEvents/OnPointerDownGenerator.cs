using UnityEngine.EventSystems;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnPointerDown))]
    public class OnPointerDownGenerator : PointerEventUnitGenerator<OnPointerDown, IPointerDownHandler>
    {
        public OnPointerDownGenerator(Unit unit) : base(unit) { }
    }
}