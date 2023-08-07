using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
[TypeIcon(typeof(Color))]
[IncludeInSettings(true)]
[Inspectable]
public struct HDRColor
{
    [Inspectable]
    [ColorUsage(true, true)]
    public Color color;
}
