using System.Collections.Generic;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.LambdaGenerator")]
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
            var parameters = string.Empty;

            if (multiLine)
            {
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
                for (int i = 0; i < parameterNames.Count; i++)
                {
                    parameters += parameterNames[i];

                    if (i < parameterNames.Count - 1)
                    {
                        parameters += ", ";
                    }
                }

                return "(" + parameters + ") => { " + body + (string.IsNullOrEmpty(body) ? string.Empty : " ") + "}";
            }
        }

        public override List<string> Usings()
        {
            return new List<string>();
        }
    }
}
