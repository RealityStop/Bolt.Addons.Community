using UnityEngine.EventSystems;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(OnBeginDrag))]
    public class OnBeginDragGenerator : PointerEventUnitGenerator<OnBeginDrag, IBeginDragHandler>
    {
        public OnBeginDragGenerator(Unit unit) : base(unit) { }
    }
}