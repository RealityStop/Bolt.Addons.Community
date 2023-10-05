using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.Humility;

[ScriptGraphExtension(typeof(ScriptGraphAsset))]
public class ScriptGraphDecorator : Decorator<ScriptGraphDecorator, ScriptGraphExtensionAttribute, ScriptGraphAsset>
{
    [Inspectable]
    public List<string> usings = new List<string>();
    [Inspectable]
    public List<ScriptGraphVariableDecleration> variables = new List<ScriptGraphVariableDecleration>();
}

[System.Serializable]
[Inspectable]
public class ScriptGraphVariableDecleration 
{
    [Inspectable]
    public string Name;
    [Inspectable]
    public Type Type;
}

public class ScriptGraphExtensionAttribute : DecoratorAttribute
{
    public ScriptGraphExtensionAttribute(Type type) : base(type)
    {
    }
}