using System.Collections.Generic;

namespace Bolt.Addons.Integrations.Continuum.CSharp
{
    public sealed class LambdaGenerator : ConstructGenerator
    {
        private bool multiLine;
        private List<string> parameterNames;
        private string body;

        private LambdaGenerator() { }

        public static LambdaGenerator SingleLine(List<string> parameterNames, string body)
        {
            var expression = new LambdaGenerator();
            expression.parameterNames = parameterNames;
            expression.body = body;
            expression.multiLine = false;
            return expression;
        }

        public static LambdaGenerator MultiLine(List<string> parameterNames, string body)
        {
            var expression = new LambdaGenerator();
            expression.parameterNames = parameterNames;
            expression.body = body;
            expression.multiLine = true;
            return expression;
        }

        public override string Generate(int indent)
        {
            if (multiLine)
            {
                var parameters = string.Empty;

                for (int i = 0; i < parameterNames.Count; i++)
                {
                    parameters += parameterNames[i];

                    if (i < parameterNames.Count - 1)
                    {
                        parameters += ", ";
                    }
                }

                return "(" + parameters + ") =>\n" +
                    CodeBuilder.Indent(indent) + "{\n" +
                    body.Replace("\n", "\n" + CodeBuilder.Indent(indent + 1)) + "\n}";
            }
            else
            {
                var parameters = string.Empty;

                for (int i = 0; i < parameterNames.Count; i++)
                {
                    parameters += parameterNames[i];

                    if (i < parameterNames.Count - 1)
                    {
                        parameters += ", ";
                    }
                }

                return "(" + parameters + ") => { " + body + "}";
            }
        }

        public override List<string> Usings()
        {
            return new List<string>();
        }
    }
}
