using UnityEngine.EventSystems;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnBeginDrag))]
    public class OnBeginDragGenerator : PointerEventUnitGenerator<OnBeginDrag, IBeginDragHandler>
    {
        public OnBeginDragGenerator(Unit unit) : base(unit) { }
    }
}