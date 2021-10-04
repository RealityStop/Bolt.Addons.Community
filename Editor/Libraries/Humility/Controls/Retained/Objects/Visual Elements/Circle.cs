using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public class Circle : ImmediateModeElement
    {
        public Color _color;
        public Color color { get => _color; set { _color = value; MarkDirtyRepaint(); } }

        public Circle(Color color)
        {
            this._color = color;
        }

        protected override void ImmediateRepaint()
        {
            var circleRadius = resolvedStyle.width / 2;
            style.backgroundColor = color;
            style.borderTopLeftRadius = circleRadius;
            style.borderTopRightRadius = circleRadius;
            style.borderBottomLeftRadius = circleRadius;
            style.borderBottomRightRadius = circleRadius;
        }
    }
}
