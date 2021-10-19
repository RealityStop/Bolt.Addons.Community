using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    /// <summary>
    /// A generator that retains data for creating an attribute as a string.
    /// </summary>
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.AttributeGenerator")]
    public sealed class AttributeGenerator : ConstructGenerator
    {
        public Type type;
        public List<object> parameterValues = new List<object>();
        public List<string> stringTypeParameterValues = new List<string>();
        public List<(string, object)> parameterValuesWithLabel = new List<(string, object)>();

        /// <summary>
        /// Generate the attribute as a string.
        /// </summary>
        public override string Generate(int indent)
        {
            var parameters = string.Empty;

            for (int i = 0; i < parameterValues.Count; i++)
            {
                parameters += parameterValues[i].As().Code(false);
                if (i < parameterValues.Count - 1) parameters += ", ";
            }

            for (int i = 0; i < parameterValuesWithLabel.Count; i++)
            {
                parameters += parameterValuesWithLabel[i].Item1 + " = " + parameterValuesWithLabel[i].Item2.As().Code(false);
                if (i < parameterValuesWithLabel.Count - 1) parameters += ", ";
            }

            for (int i = 0; i < stringTypeParameterValues.Count; i++)
            {
                parameters += "typeof(" + stringTypeParameterValues[i] + ")";
                if (i < stringTypeParameterValues.Count - 1) parameters += ", ";
            }

            var showBrackets = stringTypeParameterValues.Count > 0 || parameterValuesWithLabel.Count > 0 || parameterValues.Count > 0;

            return CodeBuilder.Indent(indent) + "[" + type.Name.Replace("Attribute", string.Empty).TypeHighlight() + (showBrackets ? "(" + parameters + ")" : string.Empty) + "]";
        }

        private AttributeGenerator()
        {

        }

        public override List<string> Usings()
        {
            var usings = new List<string>();

            if (type == null) return usings;

            if (!type.IsPrimitive) usings.Add(type.Namespace);

            for (int i = 0; i < parameterValues.Count; i++)
            {
                if (parameterValues[i] == null || parameterValues[i].GetType() == null || string.IsNullOrEmpty(parameterValues[i].GetType().Namespace))
                {
                    continue;
                }

                var @namespace = parameterValues[i].GetType().Namespace;
                if (!usings.Contains(@namespace) && !parameterValues[i].GetType().Is().PrimitiveStringOrVoid()) usings.Add(@namespace);
            }

            for (int i = 0; i < parameterValuesWithLabel.Count; i++)
            {
                if (parameterValuesWithLabel[i].Item2 == null || parameterValuesWithLabel[i].Item2.GetType() == null || string.IsNullOrEmpty(parameterValuesWithLabel[i].Item2.GetType().Namespace))
                {
                    continue;
                }
                var @namespace = parameterValuesWithLabel[i].Item2.GetType().Namespace;
                if (!usings.Contains(@namespace) && !parameterValuesWithLabel[i].Item2.GetType().Is().PrimitiveStringOrVoid()) usings.Add(@namespace);
            }

            return usings;
        }

        /// <summary>
        /// Create the attribute generator based on an existing type.
        /// </summary>
        public static AttributeGenerator Attribute<T>() where T : Attribute
        {
            return new AttributeGenerator() { type = typeof(T) };
        }

        /// <summary>
        /// Create the attribute generator based on an existing type.
        /// </summary>
        public static AttributeGenerator Attribute(Type attributeType)
        {
            return new AttributeGenerator() { type = attributeType };
        }

        /// <summary>
        /// Add a parameter to this attribute, to be a part of the final string generated between the parenthesis. Generates without a label. ("MyAttribute(10f)")
        /// </summary>
        public AttributeGenerator AddParameter(object value)
        {
            parameterValues.Add(value);
            return this;
        }

        /// <summary>
        /// Add a parameter to this attribute, to be a part of the final string generated between the parenthesis. Generates without a label. ("MyAttribute(10f)")
        /// </summary>
        public AttributeGenerator AddStringTypeParameter(string type)
        {
            stringTypeParameterValues.Add(type);
            return this;
        }

        /// <summary>
        /// Add a parameter to this attribute, to be a part of the final string generated between the parenthesis. Generates with a label ("MyAttribute(SomeLabel:10f)")
        /// </summary>
        public AttributeGenerator AddParameter(string name, object value)
        {
            parameterValuesWithLabel.Add((name, value));
            return this;
        }
    }
}