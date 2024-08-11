using Unity;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Specialized;
[NodeGenerator(typeof(Unity.VisualScripting.ForEach))]
public sealed class ForEachGenerator : LocalVariableGenerator<Unity.VisualScripting.ForEach>
{
    private string currentIndex;
    public ForEachGenerator(Unity.VisualScripting.ForEach unit) : base(unit)
    {
    }

    public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
    {
        var output = string.Empty;

        if (input == Unit.enter)
        {
            bool fallback = false;
            Type type;
            if (Unit.collection.hasValidConnection)
            {
                var connectedVariable = data.GetVariableType(GetSingleDecorator(Unit.collection.connection.source.unit as Unit, Unit.collection.connection.source.unit as Unit).variableName);
                type = connectedVariable != null ? GetElementType(connectedVariable, typeof(object)) : GetElementType(Unit.collection.connection.source.type, typeof(object));
                variableName = data.AddLocalNameInScope("item", type);
            }
            else
            {
                fallback = true;
                type = typeof(object);
                variableName = data.AddLocalNameInScope("item", typeof(object));
            }
            var collection = GenerateValue(Unit.collection, data);
            bool usesIndex = Unit.currentIndex.hasValidConnection;
            if (usesIndex)
            {
                currentIndex = data.AddLocalNameInScope("currentIndex", typeof(int));
                output += CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent) + typeof(int).As().CSharpName() + " " + currentIndex.VariableHighlight() + " = -1;\n");
            }
            output += CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent) + $"foreach".ControlHighlight() + " (" + (fallback && type == typeof(object) ? "var" : $"{type.As().CSharpName()}") + $" {variableName}".VariableHighlight() + " in ".ConstructHighlight() + $"{collection})");
            output += "\n";
            output += CodeUtility.MakeSelectable(Unit, CodeBuilder.OpenBody(indent));
            output += "\n";
            if (usesIndex)
            {
                output += CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent + 1) + currentIndex.VariableHighlight() + "++;\n");
            }
            if (Unit.body.hasAnyConnection)
            {
                data.NewScope();
                output += GetNextUnit(Unit.body, data, indent + 1);
                data.ExitScope();
                output += "\n";
            }
            
            output += CodeUtility.MakeSelectable(Unit, CodeBuilder.CloseBody(indent));
        }

        if (Unit.exit.hasAnyConnection)
        {
            output += "\n";
            output += GetNextUnit(Unit.exit, data, indent);
        }

        return output;
    }

    private Type GetElementType(Type type, Type fallback)
    {
        if (typeof(IDictionary).IsAssignableFrom(type))
        {
            if (type.IsGenericType)
            {
                return typeof(KeyValuePair<,>).MakeGenericType(type.GetGenericArguments());
            }
            return typeof(DictionaryEntry);
        }
        else if (type.IsArray)
        {
            return type.GetElementType();
        }
        else if (typeof(IList).IsAssignableFrom(type))
        {
            if (type.IsGenericType)
            {
                return type.GetGenericArguments()[0];
            }
            return typeof(object);
        }
        return fallback;
    }

    public override string GenerateValue(ValueOutput output, ControlGenerationData data)
    {
        if (output == Unit.currentItem)
        {
            if (Unit.dictionary)
            {
                return variableName.VariableHighlight() + ".Value";
            }
            return variableName.VariableHighlight();
        }
        else if (output == Unit.currentKey)
        {
            return variableName.VariableHighlight() + ".Key";
        }
        else
        {
            return currentIndex.VariableHighlight();
        }
    }

    public override string GenerateValue(ValueInput input, ControlGenerationData data)
    {
        if (input == Unit.collection)
        {
            if (input.hasValidConnection)
            {
                return CodeUtility.MakeSelectable(input.connection.source.unit as Unit, new ValueCode(GetNextValueUnit(input, data), Unit.dictionary ? typeof(IDictionary) : typeof(IEnumerable), ShouldCast(input, false, data)));
            }
        }

        return base.GenerateValue(input, data);
    }
}