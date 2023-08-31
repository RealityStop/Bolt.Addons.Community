using Unity.VisualScripting.Community.Libraries.CSharp;
using System;
using UnityEngine;
using System.Linq;
using UnityEditor;
using System.Text;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    [CodeGenerator(typeof(UnitAsset))]
    public sealed class UnitAssetGenerator : MemberTypeAssetGenerator<UnitAsset, UnitFieldDeclaration, UnitMethodDeclaration, UnitConstructorDeclaration>
    {
        string _args = string.Empty;

        private void AddDefinitionMethod(UnitAsset unitAsset)
        {
            var existingDefinitionMethod = Data.methods.Any(m => m.methodName == "Definition");

            if (!existingDefinitionMethod)
            {
                var declaration = UnitMethodDeclaration.CreateInstance(typeof(UnitMethodDeclaration)) as UnitMethodDeclaration;  
                declaration.hideFlags = HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(declaration, Data);
                declaration.modifier = MethodModifier.Override;
                declaration.scope = AccessModifier.Protected;
                declaration.methodName = "Definition";
                declaration.name = declaration.methodName;
                declaration.returnType = typeof(void);
                Data.methods.Add(declaration);
                var functionUnit = new FunctionNode(FunctionType.Method);
                functionUnit.methodDeclaration = declaration;
                declaration.graph.units.Add(functionUnit);
                var baseUnit = new Base();
                if (Data.unitType != typeof(Unit))
                { 
                    baseUnit.method = "Definition";
                    declaration.graph.units.Add(baseUnit);
                    functionUnit.invoke.ConnectToValid(baseUnit.Enter);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private void AddRegisterProperty(UnitAsset unitAsset)
        {
            bool existingRegisterProperty = unitAsset.variables.Any(p => p.name == "register");

            if (!existingRegisterProperty)
            {
                var declaration = UnitFieldDeclaration.CreateInstance(typeof(UnitFieldDeclaration)) as UnitFieldDeclaration;
                var getter = UnitFieldDeclaration.CreateInstance<PropertyGetterMacro>();
                var setter = UnitFieldDeclaration.CreateInstance<PropertySetterMacro>();
                AssetDatabase.AddObjectToAsset(declaration, Data);
                AssetDatabase.AddObjectToAsset(getter, Data);
                AssetDatabase.AddObjectToAsset(setter, Data);
                declaration.name = "register";
                declaration.set = false;
                declaration.propertyModifier = PropertyModifier.Override;
                declaration.scope = AccessModifier.Protected;
                declaration.type = typeof(bool);
                declaration.isProperty = true;
                Data.variables.Add(declaration);
                var functionGetterUnit = new FunctionNode(FunctionType.Getter);
                var functionSetterUnit = new FunctionNode(FunctionType.Setter);
                functionGetterUnit.fieldDeclaration = declaration;
                functionSetterUnit.fieldDeclaration = declaration;
                var returnNode = new Return();
                var boolUnit = new Literal(typeof(bool));
                declaration.getter = getter;
                declaration.setter = setter;
                declaration.getter.graph.units.Add(functionGetterUnit);
                declaration.getter.graph.units.Add(returnNode);
                declaration.getter.graph.units.Add(boolUnit);
                declaration.setter.graph.units.Add(functionSetterUnit);
                functionGetterUnit.invoke.ConnectToValid(returnNode.Enter);
                boolUnit.output.ConnectToValid(returnNode.Data);
                declaration.hideFlags = HideFlags.HideInHierarchy;
                getter.hideFlags = HideFlags.HideInHierarchy;
                setter.hideFlags = HideFlags.HideInHierarchy;
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private void AddBuildRelationsMethod(UnitAsset unitAsset)
        {
            bool existingBuildRelationsMethod = Data.methods.Any(m => m.methodName == "BuildRelations");

            if (!existingBuildRelationsMethod)
            {
                var declaration = UnitMethodDeclaration.CreateInstance(typeof(UnitMethodDeclaration)) as UnitMethodDeclaration;
                declaration.hideFlags = HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(declaration, Data);
                declaration.modifier = MethodModifier.Override;
                declaration.scope = AccessModifier.Protected;
                declaration.methodName = "BuildRelations";
                declaration.name = declaration.methodName;
                declaration.returnType = typeof(void);
                declaration.parameters.Add(new Utility.TypeParam() { type = typeof(ValueInput), name = "input"});
                Data.methods.Add(declaration);
                var functionUnit = new FunctionNode(FunctionType.Method);
                functionUnit.methodDeclaration = declaration;
                declaration.graph.units.Add(functionUnit);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private string GetUnitArgsDisplayName()
        {
            if (Data.unitArgs != null)
            {
                return Data.unitArgs.DisplayName();
            }
            return string.Empty;
        }

        protected override TypeGenerator OnGenerateType(ref string output, NamespaceGenerator @namespace)
        {
            string args = string.Empty;

            if (Data.unitType.ContainsGenericParameters)
            {
                args = "<" + GetUnitArgsDisplayName() + ">";
                args = args.TypeHighlight();
            }

            string originalClassName = Data.unitType.CSharpName(false);
            StringBuilder modifiedClassName = new StringBuilder();

            foreach (char c in originalClassName)
            {
                if (c == '<' || c == '>')
                {
                    modifiedClassName.Append(' ');
                }
                else
                {
                    modifiedClassName.Append(c);
                }
            }

            var @class = ClassGenerator.Class(RootAccessModifier.Public, ClassModifier.None, Data.title.LegalMemberName(), modifiedClassName.ToString().Replace(" ", "") + args, "Unity.VisualScripting.Community");
            if (Data.category.Length > 0) @class.AddAttribute(AttributeGenerator.Attribute<UnitCategory>().AddParameter(Data.category));
            if (Data.title.Length > 0) @class.AddAttribute(AttributeGenerator.Attribute<UnitTitleAttribute>().AddParameter(Data.title));

            AddDefinitionMethod(Data);

            if (Data.unitType == typeof(VariadicNode<>)) 
            {
                AddBuildRelationsMethod(Data);
            }

            if (Data.unitType == typeof(EventUnit<>))
            {
                AddRegisterProperty(Data);
            }
            else
            {
                var existingRegisterProperty = Data.variables.FirstOrDefault(p => p.name == "register");

                if (existingRegisterProperty != null)
                {
                    Data.variables.Remove(existingRegisterProperty);
                    AssetDatabase.RemoveObjectFromAsset(existingRegisterProperty);
                }
            }

            for (int i = 0; i < Data.constructors.Count; i++)
            {
                var constructor = ConstructorGenerator.Constructor(Data.constructors[i].scope, Data.constructors[i].modifier, Data.title.LegalMemberName());
                if (Data.constructors[i].graph.units.Count > 0)
                {
                    var unit = Data.constructors[i].graph.units[0] as FunctionNode;
                    constructor.Body(FunctionNodeGenerator.GetSingleDecorator(unit, unit).GenerateControl(null, new ControlGenerationData(), 0));

                    for (int pIndex = 0; pIndex < Data.constructors[i].parameters.Count; pIndex++)
                    {
                        if (!string.IsNullOrEmpty(Data.constructors[i].parameters[pIndex].name)) constructor.AddParameter(false, ParameterGenerator.Parameter(Data.constructors[i].parameters[pIndex].name, Data.constructors[i].parameters[pIndex].type, ParameterModifier.None));
                    }
                }

                @class.AddConstructor(constructor);
            }

            for (int i = 0; i < Data.variables.Count; i++)
            {
                if (!string.IsNullOrEmpty(Data.variables[i].name) && Data.variables[i].type != null)
                {
                    var attributes = Data.variables[i].attributes;

                    if (Data.variables[i].isProperty)
                    {
                        var property = PropertyGenerator.Property(Data.variables[i].scope, Data.variables[i].propertyModifier, Data.variables[i].type, Data.variables[i].name, false);

                        for (int attrIndex = 0; attrIndex < attributes.Count; attrIndex++)
                        {
                            AttributeGenerator attrGenerator = AttributeGenerator.Attribute(attributes[attrIndex].GetAttributeType());
                            property.AddAttribute(attrGenerator);
                        }

                        if (Data.variables[i].get)
                        {
                            property.MultiStatementGetter(AccessModifier.Public, NodeGenerator.GetSingleDecorator(Data.variables[i].getter.graph.units[0] as Unit, Data.variables[i].getter.graph.units[0] as Unit)
                            .GenerateControl(null, new ControlGenerationData() { returns = Data.variables[i].type }, 0));
                        }

                        if (Data.variables[i].set)
                        {
                            property.MultiStatementSetter(AccessModifier.Public, NodeGenerator.GetSingleDecorator(Data.variables[i].setter.graph.units[0] as Unit, Data.variables[i].setter.graph.units[0] as Unit)
                            .GenerateControl(null, new ControlGenerationData(), 0));
                        }

                        @class.AddProperty(property);
                    }
                    else
                    {
                        var field = FieldGenerator.Field(Data.variables[i].scope, Data.variables[i].fieldModifier, Data.variables[i].type, Data.variables[i].name);


                        for (int attrIndex = 0; attrIndex < attributes.Count; attrIndex++)
                        {
                            AttributeGenerator attrGenerator = AttributeGenerator.Attribute(attributes[attrIndex].GetAttributeType());
                            field.AddAttribute(attrGenerator);
                        }

                        @class.AddField(field);
                    }
                }
            }

            for (int i = 0; i < Data.methods.Count; i++)
            {
                if (!string.IsNullOrEmpty(Data.methods[i].name) && Data.methods[i].returnType != null)
                {
                    var method = MethodGenerator.Method(Data.methods[i].scope, Data.methods[i].modifier, Data.methods[i].returnType, Data.methods[i].name);
                    if (Data.methods[i].graph.units.Count > 0)
                    {
                        var unit = Data.methods[i].graph.units[0] as FunctionNode;
                        method.Body(FunctionNodeGenerator.GetSingleDecorator(unit, unit).GenerateControl(null, new ControlGenerationData(), 0));

                        for (int pIndex = 0; pIndex < Data.methods[i].parameters.Count; pIndex++)
                        {
                            if (!string.IsNullOrEmpty(Data.methods[i].parameters[pIndex].name)) method.AddParameter(ParameterGenerator.Parameter(Data.methods[i].parameters[pIndex].name, Data.methods[i].parameters[pIndex].type, ParameterModifier.None));
                        }
                    }

                    @class.AddMethod(method);
                }
            }

            @namespace.@namespace = Data.Namespace;
            @namespace.AddClass(@class);

            return @class;
        }
    }
}
