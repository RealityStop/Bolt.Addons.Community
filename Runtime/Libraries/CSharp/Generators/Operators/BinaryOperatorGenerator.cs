using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    /// <summary>
    /// A generator that retains data for creating a Binary Operator as a string.
    /// </summary>
    [Serializable]
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.BinaryOperatorGenerator")]
    public sealed class BinaryOperatorGenerator : ConstructGenerator
    {
        [SerializeField]
        private Operator _operator;
        public Operator @operator { get => _operator; private set => _operator = value; }

        private BinaryOperatorGenerator() { }

        /// <summary>
        /// Create the attribute generator based on an operator enum value.
        /// </summary>
        public static BinaryOperatorGenerator BinaryOperator(Operator @operator)
        {
            var binaryGenerator = new BinaryOperatorGenerator();
            binaryGenerator.@operator = @operator;
            return binaryGenerator;
        }

        /// <summary>
        /// Generate the operator as a string.
        /// </summary>
        public override string Generate(int indent)
        {
            var output = " ";

            switch (@operator)
            {
                case Operator.Add:
                    output += "+";
                    break;

                case Operator.And:
                    output += "&&";
                    break;

                case Operator.Divide:
                    output += "/";
                    break;

                case Operator.Equal:
                    output += "==";
                    break;

                case Operator.ExclusiveOr:
                    output += "^";
                    break;

                case Operator.Greater:
                    output += ">";
                    break;

                case Operator.GreaterOrEqual:
                    output += ">=";
                    break;

                case Operator.NotEqual:
                    output += "!=";
                    break;

                case Operator.LeftShift:
                    output += "<<";
                    break;

                case Operator.Less:
                    output += "<";
                    break;

                case Operator.LessOrEqual:
                    output += "<=";
                    break;

                case Operator.Modulo:
                    output += "%";
                    break;

                case Operator.Multiply:
                    output += "*";
                    break;

                case Operator.Or:
                    output += "||";
                    break;

                case Operator.RightShift:
                    output += ">>";
                    break;

                case Operator.Subtract:
                    output += "-";
                    break;
            }

            output += " ";

            return output;
        }

        public override List<string> Usings()
        {
            return new List<string>();
        }
    }
}