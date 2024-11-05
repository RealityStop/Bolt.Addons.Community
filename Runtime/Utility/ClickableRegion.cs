using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableRegion
{
    public string unitId = "";
    public string code = "";
    public int startLine;
    public int endLine;
    public bool isClickable = true;
    public ClickableRegion(string unitId, string code, int startLine, int endLine, bool isClickable = true)
    {
        this.unitId = unitId;
        this.code = code;
        this.startLine = startLine;
        this.endLine = endLine;
        this.isClickable = isClickable;
    }
}