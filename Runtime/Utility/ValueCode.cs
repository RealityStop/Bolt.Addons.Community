using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.Humility;

public class ValueCode
{
    public ValueCode(string code)
    {
        this.code = code;
    }

    public ValueCode(string code, Type castType)
    {
        this.code = code;
        this.castType = castType;
    }

    public ValueCode(string code, Type castType, bool shouldCast)
    {
        this.code = code;
        if (shouldCast)
        {
            this.castType = castType;
        }
    }

    private Type castType;
    private string code;
    public bool isCasted
    {
        get
        {
            return castType != null;
        }
    }

    public string GetCode()
    {
        var cast = isCasted ? $"(({castType.As().CSharpName(false, true)})" : string.Empty;
        var _code = cast + code + (isCasted ? ")" : string.Empty);
        return _code;
    }

    public static implicit operator string(ValueCode valueCode)
    {
        return valueCode.GetCode();
    }

    public static string operator -(ValueCode valueCode, string str)
    {
        return valueCode.GetCode().Replace(str, "");
    }
}