using UnityEngine;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community
{
    public static class RectUtility
    {
        public struct SnapResult
        {
            public bool snapped;
            public Vector2 snapPosition;
            public List<Line> snapLines;

            public struct Line
            {
                public Vector2 start;
                public Vector2 end;
            }
        }

        public static SnapResult CheckSnap(Rect current, List<Rect> others, float threshold = 5f)
        {
            SnapResult result = new SnapResult
            {
                snapped = false,
                snapPosition = current.position,
                snapLines = new List<SnapResult.Line>()
            };

            Vector2 newPos = current.position;

            SnapResult.Line? closestHorizontalLine = null;
            float closestHorizontalDistance = float.MaxValue;

            SnapResult.Line? closestVerticalLine = null;
            float closestVerticalDistance = float.MaxValue;

            foreach (var target in others)
            {
                if (target == current) continue;

                float[] currentX = { current.xMin, current.center.x, current.xMax };
                float[] targetX = { target.xMin, target.center.x, target.xMax };

                for (int ci = 0; ci < currentX.Length; ci++)
                {
                    for (int ti = 0; ti < targetX.Length; ti++)
                    {
                        float cx = currentX[ci];
                        float tx = targetX[ti];
                        float dist = Mathf.Abs(cx - tx);

                        if (dist <= threshold && dist < closestHorizontalDistance)
                        {
                            closestHorizontalDistance = dist;
                            float deltaX = tx - cx;

                            Rect moved = current;
                            moved.position += new Vector2(deltaX, 0);

                            newPos.x = moved.x;

                            float yStart = Mathf.Max(moved.yMin, target.yMin);
                            float yEnd = Mathf.Min(moved.yMax, target.yMax);

                            if (yStart > yEnd)
                            {
                                if (moved.center.y < target.center.y)
                                {
                                    yStart = moved.yMax;
                                    yEnd = target.yMin;
                                }
                                else
                                {
                                    yStart = target.yMax;
                                    yEnd = moved.yMin;
                                }
                            }

                            closestHorizontalLine = new SnapResult.Line
                            {
                                start = new Vector2(tx, yStart),
                                end = new Vector2(tx, yEnd)
                            };
                        }
                    }
                }

                float[] currentY = { current.yMin, current.center.y, current.yMax };
                float[] targetY = { target.yMin, target.center.y, target.yMax };

                for (int ci = 0; ci < currentY.Length; ci++)
                {
                    for (int ti = 0; ti < targetY.Length; ti++)
                    {
                        float cy = currentY[ci];
                        float ty = targetY[ti];
                        float dist = Mathf.Abs(cy - ty);

                        if (dist <= threshold && dist < closestVerticalDistance)
                        {
                            closestVerticalDistance = dist;
                            float deltaY = ty - cy;

                            Rect moved = current;
                            moved.position += new Vector2(0, deltaY);

                            newPos.y = moved.y;

                            float xStart = Mathf.Max(moved.xMin, target.xMin);
                            float xEnd = Mathf.Min(moved.xMax, target.xMax);

                            if (xStart > xEnd)
                            {
                                if (moved.center.x < target.center.x)
                                {
                                    xStart = moved.xMax;
                                    xEnd = target.xMin;
                                }
                                else
                                {
                                    xStart = target.xMax;
                                    xEnd = moved.xMin;
                                }
                            }

                            closestVerticalLine = new SnapResult.Line
                            {
                                start = new Vector2(xStart, ty),
                                end = new Vector2(xEnd, ty)
                            };
                        }
                    }
                }
            }

            if (closestHorizontalLine.HasValue)
            {
                result.snapLines.Add(closestHorizontalLine.Value);
                result.snapped = true;
            }

            if (closestVerticalLine.HasValue)
            {
                result.snapLines.Add(closestVerticalLine.Value);
                result.snapped = true;
            }

            result.snapPosition = newPos.PixelPerfect();
            return result;
        }
    }
}