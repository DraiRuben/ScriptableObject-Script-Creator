using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ruben.SOCreator
{
    public static class RectUtility
    {
        public static Rect SetPosition(this Rect rect, Vector2 position)
        {
            return new Rect(position, rect.size);
        }

        public static Rect SetX(this Rect rect, float x)
        {
            return new Rect(x, rect.y, rect.width, rect.height);
        }

        public static Rect SetY(this Rect rect, float y)
        {
            return new Rect(rect.x, y, rect.width, rect.height);
        }

        public static Rect SetWidth(this Rect rect, float witdh)
        {
            return new Rect(rect.x, rect.y, witdh, rect.height);
        }

        public static Rect SetHeight(this Rect rect, float height)
        {
            return new Rect(rect.x, rect.y, rect.width, height);
        }

        public static Rect Move(this Rect rect, Vector2 offset)
        {
            return new Rect(rect.position + offset, rect.size);
        }

        public static Rect MoveX(this Rect rect, float x)
        {
            return new Rect(rect.x + x, rect.y, rect.width, rect.height);
        }

        public static Rect MoveY(this Rect rect, float y)
        {
            return new Rect(rect.x, rect.y + y, rect.width, rect.height);
        }

        public static Rect Shrink(this Rect rect, float offset)
        {
            return rect.Shrink(offset, offset);
        }

        public static Rect Grow(this Rect rect, float offset)
        {
            return rect.Shrink(-offset);
        }

        public static Rect Shrink(this Rect rect, float offsetX, float offsetY)
        {
            rect.position += new Vector2(offsetX, offsetY);
            rect.size -= new Vector2(offsetX, offsetY) * 2;
            return rect;
        }

        public static Rect Grow(this Rect rect, float offsetX, float offsetY)
        {
            return rect.Shrink(-offsetX, -offsetY);
        }

        public static Rect SliceH(this Rect rect, float factor, int index)
        {
            if (factor is >= 100 or <= 0) throw new ArgumentException("factor should be between 1 and 99");
            rect.height *= factor / 100;
            rect.y += rect.height * index;
            return rect;
        }

        public static Rect SliceW(this Rect rect, float factor, int index)
        {
            if (factor is >= 100 or <= 0) throw new ArgumentException("factor should be between 1 and 99");
            rect.width *= factor / 100;
            rect.x += rect.width * index;
            return rect;
        }

        public static Rect HalfTop(this Rect rect)
        {
            return rect.SliceH(50, 0);
        }

        public static Rect HalfBottom(this Rect rect)
        {
            return rect.SliceH(50, 1);
        }

        public static Rect HalfRight(this Rect rect)
        {
            return rect.SliceW(50, 1);
        }

        public static Rect HalfLeft(this Rect rect)
        {
            return rect.SliceW(50, 0);
        }

        public static Rect RemainderH(this Rect rect, float factor)
        {
            if (factor is >= 100 or <= 0) throw new ArgumentException("factor should be between 1 and 99");
            rect.x += rect.height * factor / 100;
            rect.height *= 1f - factor / 100;
            return rect;
        }

        public static Rect RemainderW(this Rect rect, float factor)
        {
            if (factor is >= 100 or <= 0) throw new ArgumentException("factor should be between 1 and 99");
            rect.x += rect.width * factor / 100;
            rect.width *= 1f - factor / 100;
            return rect;
        }

        public static Rect[] FlexH(this Rect rect, params float[] factors)
        {
            List<Rect> rects = new List<Rect>();
            float total = factors.Sum();
            float previousPos = 0;
            for (int i = 0; i < factors.Length; i++)
            {
                float factor = factors[i] / total;
                float currentHeight = rect.height * factor;
                rects.Add(rect.SetHeight(currentHeight).MoveY(previousPos));
                previousPos += currentHeight;
            }

            return rects.ToArray();
        }

        public static Rect[] FlexW(this Rect rect, params float[] factors)
        {
            List<Rect> rects = new List<Rect>();
            float total = factors.Sum();
            float previousPos = 0;
            for (int i = 0; i < factors.Length; i++)
            {
                float factor = factors[i] / total;
                float currentWidth = rect.width * factor;
                rects.Add(rect.SetWidth(currentWidth).MoveX(previousPos));
                previousPos += currentWidth;
            }

            return rects.ToArray();
        }

        public static Rect[] Flex(this Rect rect, EDirection direction, params float[] factors)
        {
            List<Rect> rects = new List<Rect>();
            float total = factors.Sum();
            float previousOffset = GetInitialOffset(rect, direction);
            float offsetMultiplier = direction.GetDirectionOffsetMultiplier();
            for (int i = 0; i < factors.Length; i++)
            {
                float factor = factors[i] / total;
                float currentModifiedDimension = GetModifiedDimension(rect, direction) * factor;
                rects.Add(GetModifiedRect(rect, direction, currentModifiedDimension, previousOffset));
                previousOffset += offsetMultiplier * currentModifiedDimension;
            }

            return rects.ToArray();
        }

        private static Rect GetModifiedRect(
            Rect rect,
            EDirection dir,
            float modifiedDimension,
            float previousOffset)
        {
            switch (dir)
            {
                case EDirection.Bottom:
                case EDirection.Top:
                    return rect.SetHeight(modifiedDimension).MoveY(previousOffset);

                case EDirection.Left:
                case EDirection.Right:
                    return rect.SetWidth(modifiedDimension).MoveX(previousOffset);
            }

            return default;
        }

        private static float GetModifiedDimension(Rect rect, EDirection dir)
        {
            return dir switch
            {
                EDirection.Top => rect.height,
                EDirection.Bottom => rect.height,
                EDirection.Right => rect.width,
                EDirection.Left => rect.width,
                _ => 0
            };
        }

        private static float GetInitialOffset(Rect rect, EDirection dir)
        {
            return dir switch
            {
                EDirection.Top => 0,
                EDirection.Left => 0,
                EDirection.Bottom => rect.height,
                EDirection.Right => rect.width,
                _ => 0
            };
        }

        public enum EDirection
        {
            Top,
            Bottom,
            Right,
            Left
        }

        public static float GetDirectionOffsetMultiplier(this EDirection dir)
        {
            return dir switch
            {
                EDirection.Top => 1,
                EDirection.Bottom => -1,
                EDirection.Right => 1,
                EDirection.Left => -1,
                _ => 0
            };
        }
    }
}