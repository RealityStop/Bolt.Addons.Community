using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMMath
    {
        /// <summary>
        /// Begins an adding based operation on a rect.
        /// </summary>
        public static Data.AddRect Add(this Rect rect)
        {
            return new Data.AddRect(rect);
        }

        /// <summary>
        /// Adds one rect to another and returns the sum.
        /// </summary>
        public static Rect Add(this Rect A, Rect B)
        {
            return new Rect(A.x + B.x, A.y + B.y, A.width + B.width, A.height + B.height);
        }

        /// <summary>
        /// Begins a subtracting operation on a rect.
        /// </summary>
        public static Data.SubtractRect Subtract(this Rect rect)
        {
            return new Data.SubtractRect(rect);
        }

        /// <summary>
        /// Subtracts one rect from another and returns the difference.
        /// </summary>
        public static Rect Subtract(this Rect A, Rect B)
        {
            return new Rect(A.x - B.x, A.y - B.y, A.width - B.width, A.height - B.height);
        }

        /// <summary>
        /// Begins a multiplying operation on a rect.
        /// </summary>
        public static Data.MultiplyRect Multiply(this Rect rect)
        {
            return new Data.MultiplyRect(rect);
        }

        /// <summary>
        /// Mutiplies one rect by another and returns the result.
        /// </summary>
        public static Rect Multiply(this Rect A, Rect B)
        {
            return new Rect(A.x * B.x, A.y * B.y, A.width * B.width, A.height * B.height);
        }

        /// <summary>
        /// Begins a division operation on a rect.
        /// </summary>
        public static Data.DivideRect Divide(this Rect rect)
        {
            return new Data.DivideRect(rect);
        }

        /// <summary>
        /// Divides one rect by another and returns the result.
        /// </summary>
        public static Rect Divide(this Rect A, Rect B)
        {
            return new Rect(A.x / B.x, A.y / B.y, A.width / B.width, A.height / B.height);
        }

        /// <summary>
        /// Begins an operation of splitting a rect in some way.
        /// </summary>
        public static Data.SplitRect Split(this Rect rect)
        {
            return new Data.SplitRect(rect);
        }

        /// <summary>
        /// Begins an operation for setting specific values of a rect.
        /// </summary>
        public static Data.SetRect Set(this Rect rect)
        {
            return new Data.SetRect(rect);
        }

        /// <summary>
        /// Begins an operation that deals with the left side of a rect.
        /// </summary>
        public static Data.LeftRect Left(this Rect rect)
        {
            return new Data.LeftRect(rect);
        }

        /// <summary>
        /// Begins an operation that deals with the left side of a rect.
        /// </summary>
        public static Data.RightRect Right(this Rect rect)
        {
            return new Data.RightRect(rect);
        }

        /// <summary>
        /// Begins an operation that deals with the left side of a rect.
        /// </summary>
        public static Data.TopRect Top(this Rect rect)
        {
            return new Data.TopRect(rect);
        }

        /// <summary>
        /// Begins an operation that deals with the left side of a rect.
        /// </summary>
        public static Data.BottomRect Bottom(this Rect rect)
        {
            return new Data.BottomRect(rect);
        }
    }
}