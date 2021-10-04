using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMRetained_Children
    {
        /// <summary>
        /// Sets the border thickness of the element equally on all sides.
        /// </summary>
        public static VisualElement Border(this HUMRetained.Data.Set set, int amount)
        {
            set.element.style.borderBottomWidth = amount;
            set.element.style.borderTopWidth = amount;
            set.element.style.borderLeftWidth = amount;
            set.element.style.borderRightWidth = amount;
            return set.element;
        }

        /// <summary>
        /// Sets the border radius of the element equally on all corners.
        /// </summary>
        public static VisualElement Radius(this HUMRetained.Data.Set set, int amount)
        {
            set.element.style.borderBottomLeftRadius = amount;
            set.element.style.borderBottomRightRadius = amount;
            set.element.style.borderTopLeftRadius = amount;
            set.element.style.borderTopRightRadius = amount;
            return set.element;
        }

        /// <summary>
        /// Sets the slice of the element equally on all sides.
        /// </summary>
        public static VisualElement Slice(this HUMRetained.Data.Set set, int amount)
        {
            set.element.style.unitySliceBottom = amount;
            set.element.style.unitySliceTop = amount;
            set.element.style.unitySliceLeft = amount;
            set.element.style.unitySliceRight = amount;
            return set.element;
        }

        /// <summary>
        /// Sets the border thickness of the element equally on all sides.
        /// </summary>
        public static VisualElement BorderColor(this HUMRetained.Data.Set set, Color color)
        {
            set.element.style.borderBottomColor = color;
            set.element.style.borderTopColor = color;
            set.element.style.borderLeftColor = color;
            set.element.style.borderRightColor = color;
            return set.element;
        }
        public static HUMRetained.Data.SetMargin Margin(this HUMRetained.Data.Set set)
        {
            return new HUMRetained.Data.SetMargin(set);
        }

        /// <summary>
        /// Quick access to assigning an elements margin equal on all sides.
        /// </summary>
        public static void Margin(this HUMRetained.Data.Set set, int margin)
        {
            set.element.style.marginTop = margin;
            set.element.style.marginBottom = margin;
            set.element.style.marginLeft = margin;
            set.element.style.marginRight = margin;
        }

        /// <summary>
        /// Quick access to assigning an elements margin equal on all sides.
        /// </summary>
        public static void Width(this HUMRetained.Data.SetMargin setMargin, int margin)
        {
            setMargin.set.element.style.marginLeft = margin;
            setMargin.set.element.style.marginRight = margin;
        }

        /// <summary>
        /// Quick access to assigning an elements margin equal on all sides.
        /// </summary>
        public static void Height(this HUMRetained.Data.SetMargin setMargin, int margin)
        {
            setMargin.set.element.style.marginTop = margin;
            setMargin.set.element.style.marginBottom = margin;
        }

        public static HUMRetained.Data.SetPadding Padding(this HUMRetained.Data.Set set)
        {
            return new HUMRetained.Data.SetPadding(set);
        }

        /// <summary>
        /// Quick access to assigning an elements padding equal on all sides.
        /// </summary>
        public static void Padding(this HUMRetained.Data.Set set, int padding)
        {
            set.element.style.paddingTop = padding;
            set.element.style.paddingBottom = padding;
            set.element.style.paddingLeft = padding;
            set.element.style.paddingRight = padding;
        }
    }
}
