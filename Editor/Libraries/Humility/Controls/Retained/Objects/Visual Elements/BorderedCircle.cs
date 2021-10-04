using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public class BorderedCircle : Rectangle
    {
        public int thickness;
        public Color borderColor;

        public BorderedCircle(Color color, Color borderColor, int thickness) : base(color)
        {
            this.thickness = thickness;
            this.borderColor = borderColor;

            style.borderLeftColor = borderColor;
            style.borderRightColor = borderColor;
            style.borderTopColor = borderColor;
            style.borderBottomColor = borderColor;

            style.borderLeftWidth = thickness;
            style.borderRightWidth = thickness;
            style.borderTopWidth = thickness;
            style.borderBottomWidth = thickness;
        }
    }
}
