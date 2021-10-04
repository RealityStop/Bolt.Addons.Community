using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMMath
    {
        /// <summary>
        /// Structs for passing data down a math operation.
        /// </summary>
        public static partial class Data
        {
            /// <summary>
            /// Math operation data for adding to a rect.
            /// </summary>
            public struct AddRect
            {
                public Rect rect;

                public AddRect(Rect rect)
                {
                    this.rect = rect;
                }
            }

            /// <summary>
            /// Math operation data for subtracting from a rect.
            /// </summary>
            public struct SubtractRect
            {
                public Rect rect;

                public SubtractRect(Rect rect)
                {
                    this.rect = rect;
                }
            }

            /// <summary>
            /// Math operation data for multiplying part of a rect.
            /// </summary>
            public struct MultiplyRect
            {
                public Rect rect;

                public MultiplyRect(Rect rect)
                {
                    this.rect = rect;
                }
            }

            /// <summary>
            /// Math operation data for dividing part of a rect.
            /// </summary>
            public struct DivideRect
            {
                public Rect rect;

                public DivideRect(Rect rect)
                {
                    this.rect = rect;
                }
            }

            /// <summary>
            /// Math operation data for when splitting a rect.
            /// </summary>
            public struct SplitRect
            {
                public Rect rect;

                public SplitRect(Rect rect)
                {
                    this.rect = rect;
                }
            }

            /// <summary>
            /// Math operation data for when a rect is split into someting.
            /// </summary>
            public struct SplitInto
            {
                public SplitRect split;

                public SplitInto(SplitRect split)
                {
                    this.split = split;
                }
            }

            /// <summary>
            /// Math operation data for setting a value of a rect directly.
            /// </summary>
            public struct SetRect
            {
                public Rect rect;

                public SetRect(Rect rect)
                {
                    this.rect = rect;
                }
            }

            /// <summary>
            /// Math operation data for the left side of the rect
            /// </summary>
            public struct LeftRect
            {
                public Rect rect;

                public LeftRect(Rect rect)
                {
                    this.rect = rect;
                }
            }

            /// <summary>
            /// Math operation data for the right side of the rect
            /// </summary>
            public struct RightRect
            {
                public Rect rect;

                public RightRect(Rect rect)
                {
                    this.rect = rect;
                }
            }

            /// <summary>
            /// Math operation data for the top side of the rect
            /// </summary>
            public struct TopRect
            {
                public Rect rect;

                public TopRect(Rect rect)
                {
                    this.rect = rect;
                }
            }

            /// <summary>
            /// Math operation data for the bottom side of the rect
            /// </summary>
            public struct BottomRect
            {
                public Rect rect;

                public BottomRect(Rect rect)
                {
                    this.rect = rect;
                }
            }
        }
    }
}