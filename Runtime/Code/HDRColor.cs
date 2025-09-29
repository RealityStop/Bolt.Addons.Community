using System;
using UnityEngine;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    [TypeIcon(typeof(Color))]
    [IncludeInSettings(true)]
    [Inspectable]
    [RenamedFrom("HDRColor")]
    public struct HDRColor
    {
        [Inspectable]
        [ColorUsage(true, true)]
        public Color color;

        public static implicit operator Color(HDRColor hdrColor)
        {
            return hdrColor.color;
        }

        public static implicit operator HDRColor(Color color)
        {
            return new HDRColor { color = color };
        }
        
        public override string ToString()
        {
            return color.ToString();
        }
    }
}