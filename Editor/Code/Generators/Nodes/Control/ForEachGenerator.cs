using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections;
using System.Collections.Generic;
using System;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(ForEach))]
    public sealed class ForEachGenerator : LocalVariableGenerator
    {
        private ForEach Unit => unit as ForEach;
        private string currentIndex;

        public ForEachGenerator(ForEach unit) : base(unit) { }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input != Unit.enter)
                return;

            bool fallback = false;
            Type elementType;

            bool usesIndex = Unit.currentIndex.hasValidConnection;

            if (usesIndex)
            {
                currentIndex = data.AddLocalNameInScope("currentIndex", typeof(int));
                writer.CreateVariable(typeof(int), currentIndex, writer.Action(() => writer.Int(-1)));
            }

            using (writer.NewScope(data))
            {
                if (Unit.collection.hasValidConnection)
                {
                    var sourceType = GetSourceType(Unit.collection, data, writer);
                    elementType = sourceType != null ? GetElementType(sourceType, typeof(object)) : GetElementType(Unit.collection.connection.source.type, typeof(object));

                    variableName = data.AddLocalNameInScope("item", elementType, true);
                }
                else
                {
                    fallback = true;
                    elementType = typeof(object);
                    variableName = data.AddLocalNameInScope("item", typeof(object), true);
                }

                writer.WriteIndented("foreach ".ControlHighlight());
                writer.Parentheses(w =>
                {
                    if (fallback && elementType == typeof(object))
                        w.Write("var".ConstructHighlight());
                    else
                        w.Write(writer.GetTypeNameHighlighted(elementType));

                    w.Space();
                    w.Write(variableName.VariableHighlight());
                    w.Space();
                    w.Write("in ".ControlHighlight());

                    using (data.Expect(Unit.dictionary ? typeof(IDictionary) : typeof(IEnumerable)))
                    {
                        GenerateValue(Unit.collection, data, w);
                    }
                }).NewLine();

                writer.WriteLine("{");

                using (writer.Indented())
                {
                    if (usesIndex)
                    {
                        writer.WriteIndented();
                        writer.Write(currentIndex.VariableHighlight());
                        writer.Write("++;");
                        writer.NewLine();
                    }

                    if (Unit.body.hasValidConnection)
                    {
                        GenerateChildControl(Unit.body, data, writer);
                    }
                }

                writer.WriteLine("}");
            }

            if (Unit.exit.hasValidConnection)
            {
                GenerateExitControl(Unit.exit, data, writer);
            }
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (output == Unit.currentItem)
            {
                if (!data.ContainsNameInAncestorScope(variableName))
                {
                    writer.WriteErrorDiagnostic($"{variableName}, can only be used inside the loop.", $"Could not find or access {variableName}");
                    return;
                }
                if (Unit.dictionary)
                {
                    writer.GetMember(variableName.VariableHighlight(), "Value");
                }
                else
                {
                    writer.Write(variableName.VariableHighlight());
                }

                return;
            }

            if (output == Unit.currentKey)
            {
                if (!data.ContainsNameInAncestorScope(variableName))
                {
                    writer.WriteErrorDiagnostic($"{variableName}, can only be used inside the loop.", $"Could not find or access {variableName}");
                    return;
                }
                writer.GetMember(variableName.VariableHighlight(), "Key");
                return;
            }

            writer.Write(currentIndex.VariableHighlight());
        }

        protected override void GenerateValueInternal(ValueInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input == Unit.collection && input.hasValidConnection)
            {
                var sourceType = GetSourceType(Unit.collection, data, writer);
                var expected = sourceType == typeof(object) ? (Unit.dictionary ? typeof(IDictionary) : typeof(IEnumerable)) : sourceType;

                using (data.Expect(expected))
                {
                    GenerateConnectedValue(input, data, writer, false);
                }

                return;
            }

            base.GenerateValueInternal(input, data, writer);
        }

        private Type GetElementType(Type type, Type fallback)
        {
            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                if (type.IsGenericType)
                    return typeof(KeyValuePair<,>).MakeGenericType(type.GetGenericArguments());

                return typeof(DictionaryEntry);
            }

            if (type.IsArray)
                return type.GetElementType();

            if (typeof(IList).IsAssignableFrom(type))
            {
                if (type.IsGenericType)
                    return type.GetGenericArguments()[0];

                return typeof(object);
            }

            if (type.IsGenericType && typeof(IList<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
                return type.GetGenericArguments()[0];

            return fallback;
        }
    }
}
