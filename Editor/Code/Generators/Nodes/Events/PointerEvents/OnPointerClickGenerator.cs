using UnityEngine.EventSystems;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnPointerClick))]
    public class OnPointerClickGenerator : PointerEventUnitGenerator<OnPointerClick, IPointerClickHandler>
    {
        public OnPointerClickGenerator(Unit unit) : base(unit) { }
    }
}