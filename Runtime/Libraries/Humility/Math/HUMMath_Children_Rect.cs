using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMMath_Children
    {
        /// <summary>
        /// Splits a rect into a multidimensional arrary of individual rects.
        /// </summary>
        public static Rect[,] Grid(this HUMMath.Data.SplitInto into, int columns, int rows)
        {
            var averageHeight = (into.split.rect.height / rows);
            var averageWidth = (into.split.rect.width / columns);

            Rect[,] rects = (Rect[,])Array.CreateInstance(typeof(Rect), columns, rows);

            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    var rect = new Rect(
                        into.split.rect.x + (averageWidth * x),
                        into.split.rect.y + (averageHeight * y),
                        averageWidth,
                        averageHeight
                        );

                    rects[x, y] = rect;
                }
            }

            return rects;
        }

        /// <summary>
        /// Splits a rect into a series of rows.
        /// </summary>
        public static Rect[] Rows(this HUMMath.Data.SplitInto into, int count)
        {
            var averageWidth = (into.split.rect.width / count);
            var list = new List<Rect>();

            for (int i = 0; i < count; i++)
            {
                Rect output = new Rect(
                    into.split.rect.x + (averageWidth * i),
                    into.split.rect.y,
                    averageWidth,
                    into.split.rect.height
                    );

                list.Add(output);
            }

            return list.ToArray();
        }

        /// <summary>
        /// Splits a rect into a series of columns.
        /// </summary>
        public static Rect[] Columns(this HUMMath.Data.SplitInto into, int count)
        {
            var averageHeight = (into.split.rect.height / count);
            var list = new List<Rect>();

            for (int i = 0; i < count; i++)
            {
                Rect output = new Rect(
                into.split.rect.x,
                into.split.rect.y + (averageHeight * i),
                into.split.rect.width,
                averageHeight
                );

                list.Add(output);
            }

            return list.ToArray();
        }

        /// <summary>
        /// Add to a rects position and get the copy.
        /// </summary>
        public static Rect Position(this HUMMath.Data.AddRect A, Vector2 B)
        {
            var result = A.rect;
            result.x += B.x;
            result.y += B.y;
            return result;
        }

        /// <summary>
        /// Add to a rects position equally with a single float and get the copy.
        /// </summary>
        public static Rect Position(this HUMMath.Data.AddRect A, float B)
        {
            var result = A.rect;
            result.x += B;
            result.y += B;
            return result;
        }

        /// <summary>
        /// Add to a rects size and get the copy.
        /// </summary>
        public static Rect Size(this HUMMath.Data.AddRect A, Vector2 B)
        {
            var result = A.rect;
            result.width += B.x;
            result.height += B.y;
            return result;
        }

        /// <summary>
        /// Add to a rects size equally with a single float and get the copy.
        /// </summary>
        public static Rect Size(this HUMMath.Data.AddRect A, float B)
        {
            var result = A.rect;
            result.width += B;
            result.height += B;
            return result;
        }

        /// <summary>
        /// Subtract from a rects position and get the copy.
        /// </summary>
        public static Rect Position(this HUMMath.Data.SubtractRect A, Vector2 B)
        {
            var result = A.rect;
            result.x -= B.x;
            result.y -= B.y;
            return result;
        }

        /// <summary>
        /// Subtract from a rects position equally with a single float and get the copy.
        /// </summary>
        public static Rect Position(this HUMMath.Data.SubtractRect A, float B)
        {
            var result = A.rect;
            result.x -= B;
            result.y -= B;
            return result;
        }

        /// <summary>
        /// Subtract from a rects size and get the copy.
        /// </summary>
        public static Rect Size(this HUMMath.Data.SubtractRect A, Vector2 B)
        {
            var result = A.rect;
            result.width -= B.x;
            result.height -= B.y;
            return result;
        }

        /// <summary>
        /// Subtract from a rects size equally with a single float and get the copy.
        /// </summary>
        public static Rect Size(this HUMMath.Data.SubtractRect A, float B)
        {
            var result = A.rect;
            result.width -= B;
            result.height -= B;
            return result;
        }

        /// <summary>
        /// Divide the rects position and get the copy.
        /// </summary>
        public static Rect Position(this HUMMath.Data.DivideRect A, Vector2 B)
        {
            var result = A.rect;
            result.x /= B.x;
            result.y /= B.y;
            return result;
        }

        /// <summary>
        /// Divide the rects position equally with a single float and get the copy.
        /// </summary>
        public static Rect Position(this HUMMath.Data.DivideRect A, float B)
        {
            var result = A.rect;
            result.x /= B;
            result.y /= B;
            return result;
        }

        /// <summary>
        /// Divide the rects size and get the copy.
        /// </summary>
        public static Rect Size(this HUMMath.Data.DivideRect A, Vector2 B)
        {
            var result = A.rect;
            result.width /= B.x;
            result.height /= B.y;
            return result;
        }

        /// <summary>
        /// Divide the rects size equally with a single float and get the copy.
        /// </summary>
        public static Rect Size(this HUMMath.Data.DivideRect A, float B)
        {
            var result = A.rect;
            result.width /= B;
            result.height /= B;
            return result;
        }

        /// <summary>
        /// Multiply the rects position and get the copy.
        /// </summary>
        public static Rect Position(this HUMMath.Data.MultiplyRect A, Vector2 B)
        {
            var result = A.rect;
            result.x *= B.x;
            result.y *= B.y;
            return result;
        }

        /// <summary>
        /// Multiply the rects position equally with a single float and get the copy.
        /// </summary>
        public static Rect Position(this HUMMath.Data.MultiplyRect A, float B)
        {
            var result = A.rect;
            result.x *= B;
            result.y *= B;
            return result;
        }

        /// <summary>
        /// Multiply the rects size and get the copy.
        /// </summary>
        public static Rect Size(this HUMMath.Data.MultiplyRect A, Vector2 B)
        {
            var result = A.rect;
            result.width *= B.x;
            result.height *= B.y;
            return result;
        }

        /// <summary>
        /// Multiply the rects size equally with a single float and get the copy.
        /// </summary>
        public static Rect Size(this HUMMath.Data.MultiplyRect A, float B)
        {
            var result = A.rect;
            result.width *= B;
            result.height *= B;
            return result;
        }

        /// <summary>
        /// Add to X of this rect.
        /// </summary>
        public static Rect X(this HUMMath.Data.AddRect add, float value)
        {
            return add.rect.Add(new Rect(value, 0, 0, 0));
        }

        /// <summary>
        /// Add to Y of this rect.
        /// </summary>
        public static Rect Y(this HUMMath.Data.AddRect add, float value)
        {
            return add.rect.Add(new Rect(0, value, 0, 0));
        }

        /// <summary>
        /// Add to the width of this rect.
        /// </summary>
        public static Rect Width(this HUMMath.Data.AddRect add, float value)
        {
            return add.rect.Add(new Rect(0, 0, value, 0));
        }

        /// <summary>
        /// Add to the heigt of this rect.
        /// </summary>
        public static Rect Height(this HUMMath.Data.AddRect add, float value)
        {
            return add.rect.Add(new Rect(0, 0, 0, value));
        }

        /// <summary>
        /// Subtract from X of this rect.
        /// </summary>
        public static Rect X(this HUMMath.Data.SubtractRect subtract, float value)
        {
            return subtract.rect.Subtract(new Rect(value, 0, 0, 0));
        }

        /// <summary>
        /// Subtract from Y of this rect.
        /// </summary>
        public static Rect Y(this HUMMath.Data.SubtractRect subtract, float value)
        {
            return subtract.rect.Subtract(new Rect(0, value, 0, 0));
        }

        /// <summary>
        /// Subtract from the rects width.
        /// </summary>
        public static Rect Width(this HUMMath.Data.SubtractRect subtract, float value)
        {
            return subtract.rect.Subtract(new Rect(0, 0, value, 0));
        }

        /// <summary>
        /// Subtract from the rects height.
        /// </summary>
        public static Rect Height(this HUMMath.Data.SubtractRect subtract, float value)
        {
            return subtract.rect.Subtract(new Rect(0, 0, 0, value));
        }

        /// <summary>
        /// Mutiply the X axis position of this rect.
        /// </summary>
        public static Rect X(this HUMMath.Data.MultiplyRect multiply, float value)
        {
            return multiply.rect.Multiply(new Rect(value, 0, 0, 0));
        }

        /// <summary>
        /// Multiply the Y axis position of this rect.
        /// </summary>
        public static Rect Y(this HUMMath.Data.MultiplyRect multiply, float value)
        {
            return multiply.rect.Multiply(new Rect(0, value, 0, 0));
        }

        /// <summary>
        /// Multiply the width of this rect.
        /// </summary>
        public static Rect Width(this HUMMath.Data.MultiplyRect multiply, float value)
        {
            return multiply.rect.Multiply(new Rect(0, 0, value, 0));
        }

        /// <summary>
        /// Multiply the height of this rect.
        /// </summary>
        public static Rect Height(this HUMMath.Data.MultiplyRect multiply, float value)
        {
            return multiply.rect.Multiply(new Rect(0, 0, 0, value));
        }

        /// <summary>
        /// Divide the X position of this rect.
        /// </summary>
        public static Rect X(this HUMMath.Data.DivideRect divide, float value)
        {
            return divide.rect.Subtract(new Rect(value, 0, 0, 0));
        }

        /// <summary>
        /// Divide the Y position of this rect.
        /// </summary>
        public static Rect Y(this HUMMath.Data.DivideRect divide, float value)
        {
            return divide.rect.Subtract(new Rect(0, value, 0, 0));
        }

        /// <summary>
        /// Divide the width of this rect.
        /// </summary>
        public static Rect Width(this HUMMath.Data.DivideRect divide, float value)
        {
            return divide.rect.Divide(new Rect(0, 0, value, 0));
        }

        /// <summary>
        /// Divide the height of this rect.
        /// </summary>
        public static Rect Height(this HUMMath.Data.DivideRect divide, float value)
        {
            return divide.rect.Subtract(new Rect(0, 0, 0, value));
        }

        /// <summary>
        /// Set the X value of this rect.
        /// </summary>
        public static Rect X(this HUMMath.Data.SetRect set, float value)
        {
            return new Rect(value, set.rect.y, set.rect.width, set.rect.height);
        }

        /// <summary>
        /// Set the Y value of this rect.
        /// </summary>
        public static Rect Y(this HUMMath.Data.SetRect set, float value)
        {
            return new Rect(set.rect.x, value, set.rect.width, set.rect.height);
        }

        /// <summary>
        /// Set the width of this rect.
        /// </summary>
        public static Rect Width(this HUMMath.Data.SetRect set, float value)
        {
            return new Rect(set.rect.x, set.rect.y, value, set.rect.height);
        }

        /// <summary>
        /// Set the height of this rect.
        /// </summary>
        public static Rect Height(this HUMMath.Data.SetRect set, float value)
        {
            return new Rect(set.rect.x, set.rect.y, set.rect.width, value);
        }
    }
}