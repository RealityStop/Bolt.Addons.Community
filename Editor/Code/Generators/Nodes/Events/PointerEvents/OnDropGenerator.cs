using UnityEngine.EventSystems;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(OnDrop))]
    public class OnDropGenerator : PointerEventUnitGenerator<OnDrop, IDropHandler>
    {
        public OnDropGenerator(Unit unit) : base(unit) { }
    }
}