using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public class Rectangle : VisualElement
    {
        public Color _color;
        public Color color { get => _color; set { _color = value; MarkDirtyRepaint(); } }
        public int radius;

        public Rectangle(Color color, int radius = 0)
        {
            this._color = color;
            this.radius = radius;

            style.backgroundColor = color;

            style.borderTopLeftRadius = radius;
            style.borderTopRightRadius = radius;
            style.borderBottomLeftRadius = radius;
            style.borderBottomRightRadius = radius;
        }
    }
}
