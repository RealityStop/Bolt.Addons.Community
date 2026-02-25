using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public class ImageToggle : Image
    {
        public Texture2D normal;
        public Texture2D active;
        public Texture2D hovering;
        public Texture2D pressed;
        private bool isHovering;
        private bool isActive;
        private Action<bool> onPressed;

        public ImageToggle(Texture2D normal, Texture2D active, Texture2D hovering, Texture2D pressed, Action<bool> onPressed = null)
        {
            this.normal = normal;
            this.active = active;
            this.pressed = pressed;
            this.hovering = hovering;
            this.onPressed = onPressed;

            RegisterCallback<MouseEnterEvent>((e) =>
            {
                if (!isActive) style.backgroundImage = hovering;
                isHovering = true;
                MarkDirtyRepaint();
            });

            RegisterCallback<MouseLeaveEvent>((e) =>
            {
                if (!isActive) style.backgroundImage = normal;
                isHovering = false;
                MarkDirtyRepaint();
            });

            RegisterCallback<MouseDownEvent>((e) =>
            {
                if (!isActive) style.backgroundImage = pressed;
                MarkDirtyRepaint();
            });

            RegisterCallback<MouseUpEvent>((e) =>
            {
                style.backgroundImage = isActive ? active : normal;

                if (isHovering)
                {
                    isActive = !isActive;
                    this.onPressed?.DynamicInvoke(isActive);
                }
                MarkDirtyRepaint();
            });
        }

        public void Deactivate()
        {
            style.backgroundImage = normal;
            isHovering = false;
            isActive = false;
            MarkDirtyRepaint();
        }
    }
}
