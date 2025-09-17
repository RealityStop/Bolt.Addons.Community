using System;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Formula))]
    public sealed class FormulaGenerator : NodeGenerator<Formula>
    {
        private static readonly Dictionary<string, string> FunctionMap = new()
        {
            ["abs"] = $"{"Mathf".TypeHighlight()}.Abs",
            ["acos"] = $"{"Mathf".TypeHighlight()}.Acos",
            ["asin"] = $"{"Mathf".TypeHighlight()}.Asin",
            ["atan"] = $"{"Mathf".TypeHighlight()}.Atan",
            ["ceil"] = $"{"Mathf".TypeHighlight()}.Ceil",
            ["cos"] = $"{"Mathf".TypeHighlight()}.Cos",
            ["exp"] = $"{"Mathf".TypeHighlight()}.Exp",
            ["floor"] = $"{"Mathf".TypeHighlight()}.Floor",
            ["log"] = $"{"Mathf".TypeHighlight()}.Log",
            ["log10"] = $"{"Mathf".TypeHighlight()}.Log10",
            ["pow"] = $"{"Mathf".TypeHighlight()}.Pow",
            ["round"] = $"{"Mathf".TypeHighlight()}.Round",
            ["sign"] = $"{"Mathf".TypeHighlight()}.Sign",
            ["sin"] = $"{"Mathf".TypeHighlight()}.Sin",
            ["sqrt"] = $"{"Mathf".TypeHighlight()}.Sqrt",
            ["tan"] = $"{"Mathf".TypeHighlight()}.Tan",
            ["max"] = $"{"Mathf".TypeHighlight()}.Max",
            ["min"] = $"{"Mathf".TypeHighlight()}.Min"
        };

        public FormulaGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            var formula = Unit.formula;

            if (string.IsNullOrWhiteSpace(formula))
                return MakeClickableForThisUnit("/* No formula provided */".WarningHighlight());

            try
            {
                var parsed = ParseFormula(formula, data);
                return parsed;
            }
            catch (Exception ex)
            {
                return MakeClickableForThisUnit(("/* Error: " + ex.Message + " */").WarningHighlight());
            }
        }

        private string ParseFormula(string formula, ControlGenerationData data)
        {
            int index = 0;
            return ParseExpression();

            string ParseExpression()
            {
                StringBuilder sb = new();
                while (index < formula.Length)
                {
                    char ch = formula[index];

                    if (char.IsLetter(ch))
                    {
                        string identifier = ParseIdentifier();
                        if (index < formula.Length && formula[index] == '(')
                        {
                            index++;
                            var args = ParseArguments();
                            if (!FunctionMap.TryGetValue(identifier.ToLowerInvariant(), out var func))
                                throw new Exception($"Unknown function '{identifier}'");

                            sb.Append(MakeClickableForThisUnit(func + "(") + string.Join(MakeClickableForThisUnit(", "), args) + MakeClickableForThisUnit(")"));
                        }
                        else
                        {
                            sb.Append(ParseInputReference(identifier));
                        }
                    }
                    else if (ch == '-')
                    {
                        bool isUnary = false;

                        if (sb.Length == 0)
                        {
                            isUnary = true;
                        }
                        else
                        {
                            int lastIndex = sb.Length - 1;
                            while (lastIndex >= 0 && char.IsWhiteSpace(sb[lastIndex]))
                                lastIndex--;

                            if (lastIndex < 0)
                            {
                                isUnary = true;
                            }
                            else
                            {
                                char lastChar = sb[lastIndex];
                                if (lastChar == '(' || lastChar == '+' || lastChar == '-' || lastChar == '*' || lastChar == '/' || lastChar == '^' || lastChar == ',')
                                {
                                    isUnary = true;
                                }
                            }
                        }

                        if (isUnary)
                        {
                            sb.Append(MakeClickableForThisUnit("-"));
                            index++;
                        }
                        else
                        {
                            sb.Append(MakeClickableForThisUnit(" - "));
                            index++;
                        }
                    }
                    else if (ch == '+' || ch == '-' || ch == '*' || ch == '/' || ch == '^')
                    {
                        sb.Append(MakeClickableForThisUnit(" " + ch + " "));
                        index++;
                    }
                    else if (ch == '(')
                    {
                        index++;
                        sb.Append(MakeClickableForThisUnit("(") + ParseExpression() + MakeClickableForThisUnit(")"));
                        Expect(')');
                    }
                    else if (ch == ')')
                    {
                        break;
                    }
                    else if (ch == '"')
                    {
                        sb.Append(ParseStringLiteral());
                    }
                    else if (ch == '\'')
                    {
                        sb.Append(ParseCharLiteral());
                    }
                    else if (char.IsDigit(ch) || ch == '.')
                    {
                        sb.Append(ParseNumber());
                    }
                    else if (char.IsWhiteSpace(ch))
                    {
                        index++;
                    }
                    else
                    {
                        throw new Exception($"Unexpected character ' {ch} '");
                    }
                }
                return sb.ToString();
            }

            string ParseStringLiteral()
            {
                int start = index;
                index++;

                while (index < formula.Length)
                {
                    char ch = formula[index];

                    if (ch == '"')
                    {
                        index++;
                        break;
                    }

                    index++;
                }

                string strLiteral = formula.Substring(start, index - start);
                return MakeClickableForThisUnit(strLiteral.StringHighlight());
            }

            string ParseCharLiteral()
            {
                int start = index;
                index++;

                if (index + 1 < formula.Length && formula[index + 1] == '\'')
                {
                    index += 2;
                    string charLiteral = formula.Substring(start, index - start);
                    return MakeClickableForThisUnit(charLiteral.StringHighlight());
                }

                return "'".WarningHighlight();
            }

            string ParseIdentifier()
            {
                int start = index;
                while (index < formula.Length && char.IsLetter(formula[index]))
                    index++;
                return formula.Substring(start, index - start);
            }

            List<string> ParseArguments()
            {
                List<string> args = new();
                StringBuilder argBuilder = new();
                int parenDepth = 1;

                while (index < formula.Length && parenDepth > 0)
                {
                    char ch = formula[index];

                    if (ch == '(')
                    {
                        parenDepth++;
                        argBuilder.Append(ch);
                        index++;
                    }
                    else if (ch == ')')
                    {
                        parenDepth--;
                        if (parenDepth == 0)
                        {
                            args.Add(ParseArgumentExpression(argBuilder.ToString()));
                            argBuilder.Clear();
                            index++;
                            break;
                        }
                        else
                        {
                            argBuilder.Append(ch);
                            index++;
                        }
                    }
                    else if (ch == ',' && parenDepth == 1)
                    {
                        args.Add(ParseArgumentExpression(argBuilder.ToString()));
                        argBuilder.Clear();
                        index++;
                    }
                    else
                    {
                        argBuilder.Append(ch);
                        index++;
                    }
                }

                return args;
            }

            string ParseArgumentExpression(string subExpr)
            {
                int oldIndex = index;
                string oldFormula = formula;

                formula = subExpr;
                index = 0;
                string parsed = ParseExpression();

                formula = oldFormula;
                index = oldIndex;
                return parsed;
            }

            string ParseInputReference(string name)
            {
                if (name.Length == 1 && char.IsLetter(name[0]))
                {
                    int idx = Formula.GetArgumentIndex(char.ToLowerInvariant(name[0]));
                    if (idx < Unit.multiInputs.Count)
                        return GenerateValue(Unit.multiInputs[idx], data);
                }
                return MakeClickableForThisUnit(ParseVariable(name));
            }

            string ParseNumber()
            {
                int start = index;
                bool hasDot = false;
                bool hasExp = false;

                while (index < formula.Length)
                {
                    char ch = formula[index];
                    if (char.IsDigit(ch))
                    {
                        index++;
                    }
                    else if (ch == '.' && !hasDot)
                    {
                        hasDot = true;
                        index++;
                    }
                    else if ((ch == 'e' || ch == 'E') && !hasExp)
                    {
                        hasExp = true;
                        index++;
                        if (index < formula.Length && (formula[index] == '+' || formula[index] == '-'))
                            index++;
                    }
                    else
                    {
                        break;
                    }
                }

                string numberStr = formula.Substring(start, index - start);

                if (int.TryParse(numberStr, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out _))
                {
                    return MakeClickableForThisUnit(numberStr.NumericHighlight());
                }

                if (long.TryParse(numberStr, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out _))
                {
                    return MakeClickableForThisUnit((numberStr + "L").NumericHighlight());
                }

                if (float.TryParse(numberStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _))
                {
                    return MakeClickableForThisUnit((numberStr + "f").NumericHighlight());
                }

                if (double.TryParse(numberStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _))
                {
                    return MakeClickableForThisUnit(numberStr.NumericHighlight());
                }

                throw new Exception($"Invalid numeric literal '{numberStr}'");
            }

            void Expect(char expected)
            {
                if (index >= formula.Length || formula[index] != expected)
                    throw new Exception($"Expected '{expected}' at position {index}");
                index++;
            }

            string ParseVariable(string name)
            {
                if (data.ContainsNameInAnyScope(name))
                {
                    return name.VariableHighlight();
                }

                if (data.TryGetGameObject(out var gameObject))
                {
                    if (VisualScripting.Variables.Object(gameObject).IsDefined(name))
                    {
                        return typeof(VisualScripting.Variables).As().CSharpName(false, true) + $".Object({gameObject.As().Code(false, false, true, "", false, true)}).Get<{"float".ConstructHighlight()}>({name.As().Code(false)})";
                    }
                }


                if (VisualScripting.Variables.ActiveScene.IsDefined(name))
                {
                    return typeof(VisualScripting.Variables).As().CSharpName(false, true) + $".{"ActiveScene".VariableHighlight()}.Get<{"float".ConstructHighlight()}>({name.As().Code(false)})";
                }

                if (VisualScripting.Variables.Application.IsDefined(name))
                {
                    return typeof(VisualScripting.Variables).As().CSharpName(false, true) + $".{"Application".VariableHighlight()}.Get<{"float".ConstructHighlight()}>({name.As().Code(false)})";
                }

                if (VisualScripting.Variables.Saved.IsDefined(name))
                {
                    return typeof(VisualScripting.Variables).As().CSharpName(false, true) + $".{"Saved".VariableHighlight()}.Get<{"float".ConstructHighlight()}>({name.As().Code(false)})";
                }

                return name.WarningHighlight();
            }
        }
    }
}