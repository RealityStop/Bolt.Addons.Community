using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class ArrayConstructorInfoStubWriter : MethodBaseStubWriter<ConstructorInfo>
    {
        public ArrayConstructorInfoStubWriter(ConstructorInfo constructorInfo) : base(constructorInfo) { }

        public override IEnumerable<CodeStatement> GetStubStatements()
        {
            var arguments = new List<CodeExpression>();

            foreach (var parameterInfo in stub.GetParameters())
            {
                var parameterType = new CodeTypeReference(parameterInfo.UnderlyingParameterType(), CodeTypeReferenceOptions.GlobalReference);
                var argumentName = $"arg{arguments.Count}";

                yield return new CodeVariableDeclarationStatement(
                    parameterType,
                    argumentName,
                    new CodeDefaultValueExpression(parameterType)
                );

                FieldDirection direction;
                if (parameterInfo.HasOutModifier())
                    direction = FieldDirection.Out;
                else if (parameterInfo.ParameterType.IsByRef)
                    direction = FieldDirection.Ref;
                else
                    direction = FieldDirection.In;

                arguments.Add(new CodeDirectionExpression(direction, new CodeVariableReferenceExpression(argumentName)));
            }

            if (!manipulator.isPubliclyInvocable)
                yield break;

            if (stub.DeclaringType.IsArray)
            {
                var elementType = stub.DeclaringType.GetElementType();
                var elementTypeName = $"global::{elementType.FullName.Replace('+', '.')}";

                var argsSnippet = string.Join(", ", arguments.Select(a =>
                    ((CodeVariableReferenceExpression)((CodeDirectionExpression)a).Expression)?.VariableName ?? a.ToString()
                ));

                var rank = stub.DeclaringType.GetArrayRank();
                var brackets = rank == 1 ? "" : new string(',', rank - 1);

                var expression = new CodeSnippetExpression($"_ = new {elementTypeName}[{argsSnippet}]");
                yield return new CodeExpressionStatement(expression);
            }
            else
            {
                yield return new CodeExpressionStatement(
                    new CodeObjectCreateExpression(stub.DeclaringType, arguments.ToArray())
                );
            }
        }
    }
}