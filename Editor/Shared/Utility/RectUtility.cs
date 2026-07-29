using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public static class RectUtility
    {
        public struct SnapLine
        {
            public Vector2 start;
            public Vector2 end;
        }

        public struct SnapResult
        {
            public bool snappedX;
            public bool snappedY;
            public Vector2 snapPosition;
            
            public bool hasVerticalLine;
            public SnapLine verticalLine;
            
            public bool hasHorizontalLine;
            public SnapLine horizontalLine;

            public bool snapped => snappedX || snappedY;
        }

        public static SnapResult CheckSnap(Rect current, List<Rect> others, float threshold = 5f)
        {
            SnapResult result = new SnapResult
            {
                snapPosition = current.position
            };

            float bestXDist = threshold;
            float bestYDist = threshold;

            float deltaX = 0f;
            float deltaY = 0f;

            int bestXTargetIdx = -1;
            int bestYTargetIdx = -1;
            float bestXValue = 0f;
            float bestYValue = 0f;

            for (int i = 0; i < others.Count; i++)
            {
                Rect target = others[i];
                if (target == current) continue;

                for (int ci = 0; ci < 3; ci++)
                {
                    float cx = GetXPoint(current, ci);
                    for (int ti = 0; ti < 3; ti++)
                    {
                        float tx = GetXPoint(target, ti);
                        float dist = Mathf.Abs(cx - tx);

                        if (dist < bestXDist)
                        {
                            bestXDist = dist;
                            deltaX = tx - cx;
                            bestXTargetIdx = i;
                            bestXValue = tx;
                        }
                    }
                }

                for (int ci = 0; ci < 3; ci++)
                {
                    float cy = GetYPoint(current, ci);
                    for (int ti = 0; ti < 3; ti++)
                    {
                        float ty = GetYPoint(target, ti);
                        float dist = Mathf.Abs(cy - ty);

                        if (dist < bestYDist)
                        {
                            bestYDist = dist;
                            deltaY = ty - cy;
                            bestYTargetIdx = i;
                            bestYValue = ty;
                        }
                    }
                }
            }

            if (bestXTargetIdx != -1)
            {
                result.snappedX = true;
                result.snapPosition.x += deltaX;
            }

            if (bestYTargetIdx != -1)
            {
                result.snappedY = true;
                result.snapPosition.y += deltaY;
            }

            result.snapPosition = result.snapPosition.PixelPerfect();

            Rect snappedRect = new Rect(result.snapPosition, current.size);

            if (bestXTargetIdx != -1)
            {
                Rect target = others[bestXTargetIdx];
                result.hasVerticalLine = true;

                float yMin = Mathf.Min(snappedRect.yMin, target.yMin);
                float yMax = Mathf.Max(snappedRect.yMax, target.yMax);

                result.verticalLine = new SnapLine
                {
                    start = new Vector2(bestXValue, yMin),
                    end = new Vector2(bestXValue, yMax)
                };
            }

            if (bestYTargetIdx != -1)
            {
                Rect target = others[bestYTargetIdx];
                result.hasHorizontalLine = true;

                float xMin = Mathf.Min(snappedRect.xMin, target.xMin);
                float xMax = Mathf.Max(snappedRect.xMax, target.xMax);

                result.horizontalLine = new SnapLine
                {
                    start = new Vector2(xMin, bestYValue),
                    end = new Vector2(xMax, bestYValue)
                };
            }

            return result;
        }

        private static float GetXPoint(in Rect r, int index) => index switch
        {
            0 => r.xMin,
            1 => r.center.x,
            _ => r.xMax
        };

        private static float GetYPoint(in Rect r, int index) => index switch
        {
            0 => r.yMin,
            1 => r.center.y,
            _ => r.yMax
        };
    }
}