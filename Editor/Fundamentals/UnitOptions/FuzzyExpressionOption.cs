using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community 
{
    [FuzzyOption(typeof(FuzzyExpression))]
    public sealed class FuzzyExpressionOption : UnitOption<FuzzyExpression>
    {
        FlowGraph graph;
        public FuzzyExpressionOption(FuzzyExpression unit) : base(unit)
        {
        }
    
        public string currentQuery = "Math Expression";
    
        public override string headerLabel => currentQuery;
    
        protected override string Label(bool human)
        {
            return "Calculation : " + currentQuery;
        }
    
        public override void PreconfigureUnit(FuzzyExpression unit)
        {
            base.PreconfigureUnit(unit);
    
            graph = (FlowGraph)GraphWindow.active.context.graph;
            List<List<(string token, Unit unit)>> calculations = SplitIntoCalculations(unit.tokens);
            HandleExpression(calculations, unit.position);
        }
    
        private float ComputeResult(float operand1, float operand2, string operatorToken)
        {
            switch (operatorToken)
            {
                case "+":
                    return operand1 + operand2;
                case "-":
                    return operand1 - operand2;
                case "*":
                    return operand1 * operand2;
                case "/":
                    return operand1 / operand2;
                default:
                    throw new ArgumentException("Unknown operator: " + operatorToken);
            }
        }
    
        private void HandleExpression(List<List<(string token, Unit unit)>> calculations, Vector2 position)
        {
            bool firstCalculation = true;
            int index = 0;
            float operatorX = position.x;
            float operatorY = position.y;
            foreach (var token in calculations)
            {
                if (firstCalculation)
                {
                    var operatorUnit = token[1].unit;
                    graph.units.Add(operatorUnit);
                    operatorUnit.position = new Vector2(operatorX, operatorY);
                    graph.units.Add(token[0].unit);
                    (token[0].unit as Literal).output.ConnectToValid(operatorUnit.valueInputs[0]);
                    token[0].unit.position = new Vector2(operatorX - 150, operatorY - 100);
                    graph.units.Add(token[2].unit);
                    (token[2].unit as Literal).output.ConnectToValid(operatorUnit.valueInputs[1]);
                    token[2].unit.position = new Vector2(operatorX - 150, operatorY + 100);
                    firstCalculation = false;
                    operatorX += 150;
                    operatorY += 150;
                }
                else
                {
                    var previousCalculation = calculations[index - 1];
                    var previousOperator = previousCalculation.First(calc => IsOperator(calc.token)).unit;
                    var operatorUnit = token[1].unit;
                    graph.units.Add(operatorUnit);
                    operatorUnit.position = new Vector2(operatorX, operatorY);
                    previousOperator.valueOutputs.First().ConnectToValid(operatorUnit.valueInputs[0]);
                    graph.units.Add(token[2].unit);
                    (token[2].unit as Literal).output.ConnectToValid(operatorUnit.valueInputs[1]);
                    token[2].unit.position = new Vector2(operatorX - 150, operatorY + 100);
                    operatorX += 150;
                    operatorY += 150;
                }
    
                index++;
            }
        }
    
        private float HandleExpressionResult(List<List<(string token, Unit unit)>> calculations)
        {
            bool firstCalculation = true;
            int index = 0;
            float result = 0f;
            foreach (var token in calculations)
            {
                if (firstCalculation)
                {
                    result = ComputeResult((float)(token[0].unit as Literal).value, (float)(token[2].unit as Literal).value, token[1].token);
                    firstCalculation = false;
                }
                else
                {
                    result = ComputeResult(result, (float)(token[2].unit as Literal).value, token[1].token);
                }
    
                index++;
            }
    
            return result;
        }
    
        private List<List<(string, Unit)>> SplitIntoCalculations(List<string> tokens)
        {
            List<List<(string, Unit)>> calculations = new List<List<(string, Unit)>>();
            List<(string, Unit)> currentCalculation = new List<(string, Unit)>();
    
            for (int i = 0; i < tokens.Count; i++)
            {
                string token = tokens[i];
                if (IsOperator(token))
                {
                    if (currentCalculation.Count == 0)
                    {
                        currentCalculation.Add((tokens[i - 1], new Literal(typeof(float), float.Parse(tokens[i - 1]))));
                    }
                    currentCalculation.Add((token, GetOperator(token)));
                    currentCalculation.Add((tokens[i + 1], new Literal(typeof(float), float.Parse(tokens[i + 1]))));
                    i++;
                    calculations.Add(new List<(string, Unit)>(currentCalculation));
                    currentCalculation.Clear();
                }
            }
    
            return calculations;
        }
    
        private static bool IsOperator(string token)
        {
            return token == "+" || token == "-" || token == "*" || token == "/";
        }
    
        private static Unit GetOperator(string token)
        {
            return token switch
            {
                "+" => new ScalarSum(),
                "-" => new ScalarSubtract(),
                "*" => new ScalarMultiply(),
                "/" => new ScalarDivide(),
                _ => throw new ArgumentException("Unknown operator: " + token),
            };
        }
    
    
        public override bool favoritable => false;
    
        protected override string Haystack(bool human)
        {
            return "Calculation : " + currentQuery;
        }
    
        protected override int Order()
        {
            return 0;
        }
    
        public void Update(string label)
        {
            currentQuery = label;
            FillFromUnit();
        }
    
        public override string SearchResultLabel(string query)
        {
            List<List<(string token, Unit unit)>> calculations = SplitIntoCalculations(unit.tokens);
            var result = HandleExpressionResult(calculations);
            return $"{SearchUtility.HighlightQuery(Haystack(false), query)} <color=#{ColorPalette.unityForegroundDim.ToHexString()}>= {result}</color>";
        }
    } 
}