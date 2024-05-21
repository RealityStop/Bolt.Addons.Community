using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

public class CodeLine
{
    public CodeLine(string code)
    {
        this.code = code;
        shouldIndent = false;
    }

    public CodeLine(string code, int indent)
    {
        this.code = code;
        this.indent = indent;
        shouldIndent = true;
    }

    public CodeLine(string code, int indent, Type castType)
    {
        this.code = code;
        this.indent = indent;
        this.castType = castType;
        shouldIndent = true;
    }

    private Type castType;
    private string code;
    private int indent;
    public bool isCasted
    {
        get
        {
            return castType != null;
        }

        private set
        {
            isCasted = value;
        }
    }

    private bool shouldIndent;

    public string GetCode(bool newLine = true)
    {
        var cast = isCasted ? $"({castType.As().CSharpName(false, true)})" : string.Empty;
        var _code = (shouldIndent ? CodeBuilder.Indent(indent) : string.Empty) + cast + code + ";" + (newLine ? "\n" : "");
        return _code;
    }

    public static implicit operator string(CodeLine codeLine)
    {
        return codeLine.GetCode(true);
    }

    public static string operator -(CodeLine codeLine, string str)
    {
        return codeLine.GetCode(false).Replace(str, "");
    }
}