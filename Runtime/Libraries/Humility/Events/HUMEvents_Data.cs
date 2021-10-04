using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMEvents
    {
        public static partial class Data
        {
            public struct Is
            {
                public Mouse mouse;

                public Is(Mouse mouse)
                {
                    this.mouse = mouse;
                }
            }

            public struct Clicked
            {
                public Event e;
                public MouseButton mouseButton;

                public Clicked(Event e, MouseButton mouseButton)
                {
                    this.e = e;
                    this.mouseButton = mouseButton;
                }

                public enum MouseButton
                {
                    Left, Middle, Right
                }
            }
            
            public struct Mouse
            {
                public Event e;

                public Mouse(Event e)
                {
                    this.e = e;
                }
            }

            public struct Middle
            {
                public Mouse mouse;

                public Middle(Mouse mouse)
                {
                    this.mouse = mouse;
                }
            }

            public struct DoubleLeft
            {
                public Double @double;

                public DoubleLeft(Double @double)
                {
                    this.@double = @double;
                }
            }

            public struct Left
            {
                public Mouse mouse;

                public Left(Mouse mouse)
                {
                    this.mouse = mouse;
                }
            }

            public struct Right
            {
                public Mouse mouse;

                public Right(Mouse mouse)
                {
                    this.mouse = mouse;
                }
            }

            public struct Top
            {
                public Mouse mouse;

                public Top(Mouse mouse)
                {
                    this.mouse = mouse;
                }
            }

            public struct Bottom
            {
                public Mouse mouse;

                public Bottom(Mouse mouse)
                {
                    this.mouse = mouse;
                }
            }

            public struct Double
            {
                public Mouse mouse;

                public Double(Mouse mouse)
                {
                    this.mouse = mouse;
                }
            }

            public struct RightResizer
            {
                public Right right;

                public RightResizer(Right right)
                {
                    this.right = right;
                }
            }

            public struct LeftResizer
            {
                public Left left;

                public LeftResizer(Left left)
                {
                    this.left = left;
                }
            }

            public struct TopResizer
            {
                public Top top;

                public TopResizer(Top top)
                {
                    this.top = top;
                }
            }

            public struct BottomResizer
            {
                public Bottom bottom;

                public BottomResizer(Bottom bottom)
                {
                    this.bottom = bottom;
                }
            }

            public struct Inside
            {
                public Rect position;
                public Mouse mouse;

                public Inside(Rect position, Mouse mouse)
                {
                    this.position = position;
                    this.mouse = mouse;
                }
            }
        }
    }
}
