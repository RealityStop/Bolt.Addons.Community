using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community 
{
    [System.Serializable]
    [TypeIcon(typeof(Color))]
    [IncludeInSettings(true)]
    [Inspectable]
    [RenamedFrom("HDRColor")]
    public struct HDRColor
    {
        [Inspectable]
        [ColorUsage(true, true)]
        public Color color;
    } 
}
