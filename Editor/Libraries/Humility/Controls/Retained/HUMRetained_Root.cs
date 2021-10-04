using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMRetained
    {
        /// <summary>
        /// Begins setting an aspect of a visual element.
        /// </summary>
        public static Data.Set Set(this VisualElement element)
        {
            return new Data.Set(element);
        }

        /// <summary>
        /// Quick access to assigning an elements alignment.
        /// </summary>
        public static void Align(this VisualElement element, Align alignment)
        {
            element.style.alignContent = alignment;
        }

        /// <summary>
        /// Quick access to assigning an elements item alignment.
        /// </summary>
        public static void AlignItems(this VisualElement element, Align alignment)
        {
            element.style.alignItems = alignment;
        }


        /// <summary>
        /// Quick access to assigning an elements align self.
        /// </summary>
        public static void AlignSelf(this VisualElement element, Align alignment)
        {
            element.style.alignSelf = alignment;
        }

        /// <summary>
        /// Quick access to assigning an elements justifyContent.
        /// </summary>
        public static void Justification(this VisualElement element, Justify justify)
        {
            element.style.justifyContent = justify;
        }

        /// <summary>
        /// Quick access to assigning an elements position.
        /// </summary>
        public static void PositionStyle(this VisualElement element, Position position)
        {
            element.style.position = position;
        }

        /// <summary>
        /// Quick access to assigning an elements item alignment.
        /// </summary>
        public static void Flex(this VisualElement element, FlexDirection flex)
        {
            element.style.flexDirection = flex;
        }


        /// <summary>
        /// A rectangle that behaves like a button.
        /// </summary>
        public static Rectangle RectangleButton(Color color, Color pressed, Color hover, Action onClicked)
        {
            var rectangle = new Rectangle(color);
            rectangle.RegisterCallback<MouseOverEvent>((e) => { rectangle.color = hover; });
            rectangle.RegisterCallback<MouseOutEvent>((e) => { rectangle.color = color; });
            rectangle.RegisterCallback<MouseDownEvent>((e) => { rectangle.color = pressed; });
            rectangle.RegisterCallback<MouseUpEvent>((e) => { rectangle.color = hover; onClicked?.Invoke(); });

            return rectangle;
        }

        /// <summary>
        /// Creates a visual element stretched to fill the parent.
        /// </summary>
        public static VisualElement Container()
        {
            var element = new VisualElement();
            element.StretchToParentSize();
            return element;
        }
    }
}
