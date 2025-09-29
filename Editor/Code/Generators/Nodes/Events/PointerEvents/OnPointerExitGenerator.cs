using UnityEngine.EventSystems;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnPointerExit))]
    public class OnPointerExitGenerator : PointerEventUnitGenerator<OnPointerExit, IPointerExitHandler>
    {
        public OnPointerExitGenerator(Unit unit) : base(unit) { }
    }
}