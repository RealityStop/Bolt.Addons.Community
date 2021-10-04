using Unity.VisualScripting.Community.Libraries.CSharp;
using System;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [CodeGenerator(typeof(InterfaceAsset))]
    [Serializable]
    public sealed class InterfaceAssetGenerator : CodeGenerator<InterfaceAsset>
    {
        public override string Generate(int indent)
        {
            if (Data != null)
            {
                var output = string.Empty;
                NamespaceGenerator @namespace = NamespaceGenerator.Namespace(Data.category);
                InterfaceGenerator @interface = InterfaceGenerator.Interface(Data.title.LegalMemberName());

                if (string.IsNullOrEmpty(Data.title)) return output;

                for (int i = 0; i < Data.variables.Count; i++)
                {
                    var prop = Data.variables[i];
                    if (string.IsNullOrEmpty(prop.name) || prop.type == null) continue;
                    if (!prop.get && !prop.set) prop.get = true;
                    @interface.AddProperty(InterfacePropertyGenerator.Property(prop.name, prop.type, prop.get, prop.set));
                }

                for (int i = 0; i < Data.methods.Count; i++)
                {
                    var method = Data.methods[i];
                    if (string.IsNullOrEmpty(method.name) || method.returnType == null) continue;
                    var methodGen = InterfaceMethodGenerator.Method(method.name, method.returnType);

                    for (int paramIndex = 0; paramIndex < Data.methods[i].parameters.Count; paramIndex++)
                    {
                        var parameter = Data.methods[i].parameters[paramIndex];
                        if (string.IsNullOrEmpty(parameter.name) || parameter.type == null) continue;
                        methodGen.AddParameter(ParameterGenerator.Parameter(parameter.name, parameter.type, parameter.modifier));
                    }

                    @interface.AddMethod(methodGen);
                }

#if VISUAL_SCRIPTING_1_7
                if (Data.lastCompiledName != Data.GetFullTypeName() && !string.IsNullOrEmpty(Data.GetFullTypeName()) && !string.IsNullOrEmpty(Data.lastCompiledName))
                {
                    @interface.AddAttribute(AttributeGenerator.Attribute<RenamedFromAttribute>().AddParameter(Data.lastCompiledName));
                }
#endif

                @namespace.AddInterface(@interface);
                return @namespace.Generate(indent);
            }

            return string.Empty;
        }
    }
}
