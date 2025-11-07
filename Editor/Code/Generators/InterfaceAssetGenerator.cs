using Unity.VisualScripting.Community.Libraries.CSharp;
using System;
using Unity.VisualScripting;
using System.Linq;

namespace Unity.VisualScripting.Community.CSharp
{
    [CodeGenerator(typeof(InterfaceAsset))]
    [Serializable]
    public sealed class InterfaceAssetGenerator : CodeGenerator<InterfaceAsset>
    {
        public override ControlGenerationData CreateGenerationData()
        {
            return new ControlGenerationData(typeof(object), null);
        }

        public override string Generate(int indent)
        {
            if (Data != null)
            {
                var output = string.Empty;
                NamespaceGenerator @namespace = NamespaceGenerator.Namespace(Data.category);
                InterfaceGenerator @interface = InterfaceGenerator.Interface(Data.title.LegalMemberName(), Data.interfaces.Select(@interface => @interface.type).ToArray());

                if (string.IsNullOrEmpty(Data.title)) return output;

                for (int i = 0; i < Data.variables.Count; i++)
                {
                    var prop = Data.variables[i];
                    if (string.IsNullOrEmpty(prop.name) || prop.type == null) continue;
                    if (!prop.get && !prop.set) prop.get = true;
                    @interface.AddProperty(InterfacePropertyGenerator.Property(prop.name, prop.type.type, prop.get, prop.set));
                }

                for (int i = 0; i < Data.methods.Count; i++)
                {
                    var method = Data.methods[i];
                    if (string.IsNullOrEmpty(method.name) || method.returnType == null) continue;
                    var methodGen = InterfaceMethodGenerator.Method(method.name, method.returnType.type);

                    for (int paramIndex = 0; paramIndex < Data.methods[i].parameters.Count; paramIndex++)
                    {
                        var parameter = Data.methods[i].parameters[paramIndex];
                        if (string.IsNullOrEmpty(parameter.name) || parameter.type == null) continue;
                        var param = ParameterGenerator.Parameter(parameter.name, parameter.Paramtype.type, parameter.modifier, parameter.hasDefault, parameter.defaultValue);
                        foreach (var attribute in parameter.attributes)
                        {
                            param.attributes.Add(attribute);
                        }
                        methodGen.AddParameter(param);
                    }

                    @interface.AddMethod(methodGen);
                }

                foreach (var name in Data.lastCompiledNames)
                {
                    if (!string.IsNullOrEmpty(Data.GetFullTypeName()) && name != Data.GetFullTypeName())
                    {
                        if (!string.IsNullOrEmpty(name))
                            @interface.AddAttribute(AttributeGenerator.Attribute<RenamedFromAttribute>().AddParameter(name));
                    }
                }

                @namespace.AddInterface(@interface);
                return @namespace.Generate(indent);
            }

            return string.Empty;
        }
    }
}
