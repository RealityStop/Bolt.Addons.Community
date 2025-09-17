using UnityEngine.EventSystems;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnPointerEnter))]
    public class OnPointerEnterGenerator : PointerEventUnitGenerator<OnPointerEnter, IPointerEnterHandler>
    {
        public OnPointerEnterGenerator(Unit unit) : base(unit) { }
    }
}