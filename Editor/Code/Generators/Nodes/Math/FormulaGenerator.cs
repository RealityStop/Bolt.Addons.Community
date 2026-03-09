using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Formula))]
    public sealed class FormulaGenerator : NodeGenerator<Formula>
    {
        private static readonly Dictionary<string, string> FunctionMap = new Dictionary<string, string>()
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

        public override IEnumerable<string> GetNamespaces()
        {
            yield return "Unity.VisualScripting";
            yield return "UnityEngine";
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (string.IsNullOrWhiteSpace(Unit.formula))
            {
                writer.Error("No formula provided");
                return;
            }

            try
            {
                int index = 0;
                string formula = Unit.formula;

                ParseExpression(writer);

                void ParseExpression(CodeWriter cw = null)
                {
                    cw ??= writer;

                    while (index < formula.Length)
                    {
                        char ch = formula[index];

                        if (char.IsLetter(ch))
                        {
                            string identifier = ParseIdentifier();
                            if (index < formula.Length && formula[index] == '(')
                            {
                                index++;
                                cw.Write(FunctionMap.TryGetValue(identifier.ToLowerInvariant(), out var func) ? func + "(" : identifier + "(");
                                ParseArguments(cw);
                                cw.Write(")");
                            }
                            else
                            {
                                ParseInputReference(identifier, cw);
                            }
                        }
                        else if ("-+*/^".Contains(ch))
                        {
                            cw.Write(" " + ch + " ");
                            index++;
                        }
                        else if (ch == '(')
                        {
                            index++;
                            cw.Write("(");
                            ParseExpression(cw);
                            Expect(')');
                            cw.Write(")");
                        }
                        else if (ch == ')')
                        {
                            return;
                        }
                        else if (ch == '"')
                        {
                            cw.Write(ParseStringLiteral());
                        }
                        else if (ch == '\'')
                        {
                            cw.Write(ParseCharLiteral());
                        }
                        else if (char.IsDigit(ch) || ch == '.')
                        {
                            cw.Write(ParseNumber());
                        }
                        else
                        {
                            index++;
                        }
                    }
                }

                string ParseIdentifier()
                {
                    int start = index;
                    while (index < formula.Length && char.IsLetter(formula[index])) index++;
                    return formula.Substring(start, index - start);
                }

                void ParseArguments(CodeWriter cw)
                {
                    int parenDepth = 1;
                    int argStart = index;
                    bool firstArg = true;

                    while (index < formula.Length && parenDepth > 0)
                    {
                        char ch = formula[index];

                        if (ch == '(') parenDepth++;
                        else if (ch == ')') parenDepth--;
                        else if (ch == ',' && parenDepth == 1)
                        {
                            if (!firstArg) cw.Write(", ");
                            firstArg = false;

                            string subExpr = formula.Substring(argStart, index - argStart);
                            ParseArgumentExpression(subExpr, cw);

                            index++;
                            argStart = index;
                            continue;
                        }

                        index++;
                    }

                    if (index > argStart)
                    {
                        if (!firstArg) cw.Write(", ");
                        string subExpr = formula.Substring(argStart, index - argStart - 1);
                        ParseArgumentExpression(subExpr, cw);
                    }
                }

                void ParseArgumentExpression(string subExpr, CodeWriter cw)
                {
                    string oldFormula = formula;
                    int oldIndex = index;

                    formula = subExpr;
                    index = 0;

                    ParseExpression(cw);

                    formula = oldFormula;
                    index = oldIndex;
                }

                void ParseInputReference(string name, CodeWriter cw)
                {
                    if (name.Length == 1 && char.IsLetter(name[0]))
                    {
                        int idx = Formula.GetArgumentIndex(char.ToLowerInvariant(name[0]));
                        if (idx < Unit.multiInputs.Count)
                        {
                            GenerateValue(Unit.multiInputs[idx], data, cw);
                            return;
                        }
                    }
                    ParseVariable(name, cw);
                }

                string ParseStringLiteral()
                {
                    int start = index;
                    index++;
                    while (index < formula.Length && formula[index] != '"') index++;
                    index++;
                    return formula.Substring(start, index - start).StringHighlight();
                }

                string ParseCharLiteral()
                {
                    int start = index;
                    index++;
                    if (index + 1 < formula.Length && formula[index + 1] == '\'') { index += 2; return formula.Substring(start, index - start).StringHighlight(); }
                    return "'".ErrorHighlight();
                }

                string ParseNumber()
                {
                    int start = index;
                    bool hasDot = false, hasExp = false;
                    while (index < formula.Length)
                    {
                        char ch = formula[index];
                        if (char.IsDigit(ch)) index++;
                        else if (ch == '.' && !hasDot) { hasDot = true; index++; }
                        else if ((ch == 'e' || ch == 'E') && !hasExp) { hasExp = true; index++; if (index < formula.Length && ("+-".Contains(formula[index]))) index++; }
                        else break;
                    }
                    string numberStr = formula.Substring(start, index - start);

                    if (int.TryParse(numberStr, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out _))
                        return numberStr.NumericHighlight();
                    if (long.TryParse(numberStr, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out _))
                        return (numberStr + "L").NumericHighlight();
                    if (float.TryParse(numberStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _))
                        return (numberStr + "f").NumericHighlight();
                    if (double.TryParse(numberStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _))
                        return numberStr.NumericHighlight();

                    throw new Exception($"Invalid numeric literal '{numberStr}'");
                }

                void Expect(char expected)
                {
                    if (index >= formula.Length || formula[index] != expected)
                        throw new Exception($"Expected '{expected}' at position {index}");
                    index++;
                }

                void ParseVariable(string name, CodeWriter cw)
                {
                    if (data.ContainsNameInAncestorScope(name))
                    {
                        cw.GetVariable(name);
                        return;
                    }

                    if (data.TryGetGameObject(out var go))
                    {
                        if (VisualScripting.Variables.Object(go).IsDefined(name))
                        {
                            cw.Write(typeof(VisualScripting.Variables)).Write(
                                     $".Object({writer.ObjectString(go)}).Get<{"float".ConstructHighlight()}>({writer.ObjectString(name)})");
                            return;
                        }
                    }

                    if (VisualScripting.Variables.ActiveScene.IsDefined(name))
                    {
                        cw.Write(typeof(VisualScripting.Variables)).Write(
                                 $".{"ActiveScene".VariableHighlight()}.Get<{"float".ConstructHighlight()}>({writer.ObjectString(name)})");
                        return;
                    }

                    if (VisualScripting.Variables.Application.IsDefined(name))
                    {
                        cw.Write(typeof(VisualScripting.Variables)).Write(
                                 $".{"Application".VariableHighlight()}.Get<{"float".ConstructHighlight()}>({writer.ObjectString(name)})");
                        return;
                    }

                    if (VisualScripting.Variables.Saved.IsDefined(name))
                    {
                        cw.Write(typeof(VisualScripting.Variables)).Write(
                                 $".{"Saved".VariableHighlight()}.Get<{"float".ConstructHighlight()}>({writer.ObjectString(name)})");
                        return;
                    }

                    using (cw.CodeDiagnosticScope($"Variable '{name}' could not be found!", CodeDiagnosticKind.Error))
                    {
                        cw.Write(name.ErrorHighlight());
                    }
                }
            }
            catch (Exception ex)
            {
                using (writer.CodeDiagnosticScope(ex.Message, CodeDiagnosticKind.Error))
                {
                    writer.Error("Error");
                }
            }
        }
    }
}