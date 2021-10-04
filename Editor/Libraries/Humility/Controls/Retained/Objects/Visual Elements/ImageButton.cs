using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public class ImageButton : VisualElement
    {
        public Texture2D normal;
        public Texture2D hovering;
        public Texture2D pressed;
#pragma warning disable 0414
        private bool isHovering;
#pragma warning restore 0414
        private Action onPressed;

        public ImageButton(Action onPressed, Texture2D normal, Texture2D hovering, Texture2D pressed)
        {
            this.onPressed = onPressed;

            this.normal = normal;
            this.hovering = hovering;
            this.pressed = pressed;

            style.backgroundImage = normal;
            style.backgroundColor = Color.clear;
            this.Set().Margin(4);
            this.Set().Border(0);
            
            RegisterCallback<MouseEnterEvent>((e) =>
            {
                style.backgroundImage = hovering;
                isHovering = true;
            });

            RegisterCallback<MouseLeaveEvent>((e) =>
            {
                style.backgroundImage = normal;
                isHovering = false;
            });

            RegisterCallback<MouseDownEvent>((e) =>
            {
                style.backgroundImage = pressed;
            });

            RegisterCallback<MouseUpEvent>((e) =>
            {
                style.backgroundImage = hovering;
                this.onPressed?.Invoke();
            });
        }
    }
}
