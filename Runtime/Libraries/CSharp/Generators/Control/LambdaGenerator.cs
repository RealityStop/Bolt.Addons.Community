using System.Collections.Generic;
using Unity.VisualScripting.Community.CSharp;

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

        public override void Generate(CodeWriter writer, ControlGenerationData data)
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

                writer.Write("(" + parameters + ") =>");
                writer.NewLine();

                writer.WriteLine("{");

                writer.Write(body);
                
                writer.NewLine();

                writer.WriteIndented("}");
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

                writer.Write("(" + parameters + ") => { " + body + (string.IsNullOrEmpty(body) ? string.Empty : " ") + "}");
            }
        }

        public override List<string> Usings()
        {
            return new List<string>();
        }
    }
}
