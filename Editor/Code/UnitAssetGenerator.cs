using Unity.VisualScripting.Community.Libraries.CSharp;
using System;
using UnityEngine;
using System.Linq;
using UnityEditor;
using System.Text;
using System.Collections.Generic;
using UnityEngine.XR;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    [CodeGenerator(typeof(UnitAsset))]
    public sealed class UnitAssetGenerator : MemberTypeAssetGenerator<UnitAsset, UnitFieldDeclaration, UnitMethodDeclaration, UnitConstructorDeclaration>
    {
        string _args = string.Empty;

        [DoNotSerialize]
        private bool Initalized;

        private HashSet<string> AddedProperties = new HashSet<string>();
        private HashSet<string> AddedMethods = new HashSet<string>();

        MethodInfo[] methods;

        private void AddProperty(UnitAsset unitAsset, PropertyInfo propertyInfo)
        {
            if (AddedProperties.Contains(propertyInfo.Name))
                return;
            var declaration = CreatePropertyDeclaration(propertyInfo);
            unitAsset.variables.Add(declaration);
            AddedProperties.Add(propertyInfo.Name);
        }

        private void AddMethod(UnitAsset unitAsset, MethodInfo MethodInfo)
        {
            if (AddedMethods.Contains(MethodInfo.Name) || unitAsset.methods.Any(method => method.methodName == MethodInfo.Name))
                return;

            var declaration = CreateMethodDeclaration(MethodInfo);
            unitAsset.methods.Add(declaration);
            AddedMethods.Add(MethodInfo.Name);
        }

        private UnitFieldDeclaration CreatePropertyDeclaration(PropertyInfo propertyInfo)
        {
            var declaration = UnitFieldDeclaration.CreateInstance(typeof(UnitFieldDeclaration)) as UnitFieldDeclaration;
            var getter = UnitFieldDeclaration.CreateInstance<PropertyGetterMacro>();
            var setter = UnitFieldDeclaration.CreateInstance<PropertySetterMacro>();
            AssetDatabase.AddObjectToAsset(getter, Data);
            AssetDatabase.AddObjectToAsset(setter, Data);
            declaration.getter = getter;
            declaration.setter = setter;
            declaration.isProperty = true;
            declaration.propertyModifier = PropertyModifier.Override;
            declaration.name = propertyInfo.Name;
            var functionGetterUnit = new FunctionNode(FunctionType.Getter);
            var functionSetterUnit = new FunctionNode(FunctionType.Setter);
            var GetterReturnNode = new Return();
            var SetterReturnNode = new Return();
            var GettervalueUnit = new Literal(propertyInfo.PropertyType);
            var SettervalueUnit = new Literal(propertyInfo.PropertyType);
            declaration.getter.graph.units.Add(functionGetterUnit);
            if (propertyInfo.GetMethod != null)
            {
                declaration.get = true;
                functionGetterUnit.fieldDeclaration = declaration;
                declaration.getter.graph.units.Add(GetterReturnNode);
                declaration.getter.graph.units.Add(GettervalueUnit);
                functionGetterUnit.invoke.ConnectToValid(GetterReturnNode.Enter);
                GettervalueUnit.output.ConnectToValid(GetterReturnNode.Data);

                if (propertyInfo.GetMethod.IsPublic)
                {
                    declaration.scope = AccessModifier.Public;
                }
                else if (propertyInfo.GetMethod.IsPrivate)
                {
                    declaration.scope = AccessModifier.Private;
                }
                else if (propertyInfo.GetMethod.IsFamily)
                {
                    declaration.scope = AccessModifier.Protected;
                }
            }
            else
            {
                declaration.get = false;
            }

            declaration.setter.graph.units.Add(functionSetterUnit);

            if (propertyInfo.SetMethod != null)
            {
                declaration.set = true;
                functionSetterUnit.fieldDeclaration = declaration;
                declaration.setter.graph.units.Add(SetterReturnNode);
                declaration.setter.graph.units.Add(SettervalueUnit);
                functionSetterUnit.invoke.ConnectToValid(SetterReturnNode.Enter);
                SettervalueUnit.output.ConnectToValid(SetterReturnNode.Data);

                if (propertyInfo.SetMethod.IsPublic)
                {
                    declaration.scope = AccessModifier.Public;
                }
                else if (propertyInfo.SetMethod.IsPrivate)
                {
                    declaration.scope = AccessModifier.Private;
                }
                else if (propertyInfo.SetMethod.IsFamily)
                {
                    declaration.scope = AccessModifier.Protected;
                }
            }
            else
            {
                declaration.set = false;
            }

            declaration.type = propertyInfo.PropertyType;
            declaration.hideFlags = HideFlags.HideInHierarchy;
            getter.hideFlags = HideFlags.HideInHierarchy;
            setter.hideFlags = HideFlags.HideInHierarchy;

            AssetDatabase.AddObjectToAsset(declaration, Data);

            return declaration;
        }

        private UnitMethodDeclaration CreateMethodDeclaration(MethodInfo MethodInfo)
        {
            var declaration = UnitMethodDeclaration.CreateInstance(typeof(UnitMethodDeclaration)) as UnitMethodDeclaration;

            AssetDatabase.AddObjectToAsset(declaration, Data);
            declaration.hideFlags = HideFlags.HideInHierarchy;
            declaration.modifier = MethodModifier.Override;
            declaration.scope = MethodInfo.GetScope();
            declaration.methodName = MethodInfo.Name;
            declaration.name = declaration.methodName;
            declaration.returnType = MethodInfo.ReturnType;
            Data.methods.Add(declaration);
            var functionUnit = new FunctionNode(FunctionType.Method);
            functionUnit.methodDeclaration = declaration;
            declaration.graph.units.Add(functionUnit);
            var baseUnit = new Base();

            if (Data.unitType != typeof(Unit))
            {
                baseUnit.method = MethodInfo.Name;
                declaration.graph.units.Add(baseUnit);
                functionUnit.invoke.ConnectToValid(baseUnit.Enter);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return declaration;
        }

        private void UpdateMethods(UnitAsset unitAsset, Type type)
        {
            RemoveMethods(unitAsset);

            MethodInfo[] allMethods = type.GetMethods(
                BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic)
                .Where(methodInfo => methodInfo.IsAbstract)
                .ToArray();

            foreach (var methodInfo in allMethods)
            {
                if (!methodInfo.Name.StartsWith("get_") && !methodInfo.Name.StartsWith("set_") && !AddedMethods.Any(method => method == methodInfo.Name))
                {
                    AddMethod(unitAsset, methodInfo);
                }
            }
        }


        private void RemoveMethods(UnitAsset unitAsset)
        {
            foreach (var methodName in AddedMethods)
            {
                var method = unitAsset.methods.FirstOrDefault(m => m.methodName == methodName);
                if (method != null)
                {
                    unitAsset.methods.Remove(method);
                }
            }

            AddedMethods.Clear();
        }

        private void UpdateProperties(UnitAsset unitAsset, Type type)
        {
            PropertyInfo[] allProperties = type.GetProperties(
            BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var propertyInfo in allProperties)
            {
                if (propertyInfo.GetMethod?.IsAbstract == true || propertyInfo.SetMethod?.IsAbstract == true)
                {
                    AddProperty(unitAsset, propertyInfo);
                }
            }
        }

        private void RemoveProperties(UnitAsset unitAsset)
        {
            foreach (var propertyName in AddedProperties)
            {
                var property = unitAsset.variables.FirstOrDefault(p => p.name == propertyName);
                if (property != null)
                {
                    unitAsset.variables.Remove(property);
                }
            }

            AddedProperties.Clear();
        }

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

        private void AddRequirements(UnitAsset unitAsset, Type type)
        {

            if (type != null)
            {
                MethodInfo[] allMethods = type.GetMethods();

                if (allMethods != null)
                {
                    methods = type.GetMethods().Where(method => method.IsAbstract).ToArray();
                }
            }

            foreach (var methodInfo in methods)
            {
                if (!unitAsset.methods.Any(m => m.methodName == methodInfo.Name))
                {
                    var declaration = UnitMethodDeclaration.CreateInstance(typeof(UnitMethodDeclaration)) as UnitMethodDeclaration;
                    declaration.hideFlags = HideFlags.HideInHierarchy;
                    AssetDatabase.AddObjectToAsset(declaration, Data);
                    declaration.modifier = MethodModifier.Override;
                    if (methodInfo.IsPublic)
                    {
                        declaration.scope = AccessModifier.Public;
                    }
                    else if (methodInfo.IsPrivate)
                    {
                        declaration.scope = AccessModifier.Private;
                    }
                    else if (methodInfo.IsFamily)
                    {
                        declaration.scope = AccessModifier.Protected;
                    }
                    declaration.methodName = methodInfo.Name;
                    declaration.name = declaration.methodName;
                    declaration.returnType = methodInfo.ReturnType;
                    Data.methods.Add(declaration);
                    var functionUnit = new FunctionNode(FunctionType.Method);
                    functionUnit.methodDeclaration = declaration;
                    declaration.graph.units.Add(functionUnit);
                    AddedMethods.Add(methodInfo.Name);
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void RemoveRequirements(UnitAsset unitAsset)
        {
            foreach (var propertyName in AddedProperties)
            {
                var property = unitAsset.variables.First(property => property.name == propertyName);

                if (property != null)
                {
                    unitAsset.variables.Remove(property);
                }
            }

            foreach (var methodName in AddedMethods)
            {
                var method = unitAsset.methods.First(method => method.methodName == methodName);

                if (method != null)
                {
                    unitAsset.methods.Remove(method);
                }
            }

            AddedProperties.Clear();
            AddedMethods.Clear();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void AddRegisterProperty(UnitAsset unitAsset)
        {
            bool existingRegisterProperty = unitAsset.variables.Any(p => p.name == "register");

            if (existingRegisterProperty)
            {
                var property = unitAsset.variables.First(p => p.name == "register");

                if (!property.isProperty)
                {
                    unitAsset.variables.Remove(property);
                    existingRegisterProperty = false;
                }
            }

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
                declaration.parameters.Add(new Utility.TypeParam() { type = typeof(ValueInput), name = "input" });
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

            var usings = new List<string>();

            foreach (var Constructor in Data.constructors)
            {
                foreach (Unit unit in Constructor.graph.units)
                {
                    if (!usings.Contains(NodeGenerator.GetSingleDecorator(unit, unit).NameSpace) && NodeGenerator.GetSingleDecorator(unit, unit).NameSpace != string.Empty)
                    {
                        usings.Add(NodeGenerator.GetSingleDecorator(unit, unit).NameSpace);
                        NodeGenerator.GetSingleDecorator(unit, unit).hasNamespace = true;
                    }
                }
            }

            foreach (var Variable in Data.variables)
            {
                foreach (Unit unit in Variable.getter.graph.units)
                {
                    if (!usings.Contains(NodeGenerator.GetSingleDecorator(unit, unit).NameSpace) && NodeGenerator.GetSingleDecorator(unit, unit).NameSpace != string.Empty)
                    {
                        usings.Add(NodeGenerator.GetSingleDecorator(unit, unit).NameSpace);
                        NodeGenerator.GetSingleDecorator(unit, unit).hasNamespace = true;
                    }
                }

                foreach (Unit unit in Variable.setter.graph.units)
                {
                    if (!usings.Contains(NodeGenerator.GetSingleDecorator(unit, unit).NameSpace) && NodeGenerator.GetSingleDecorator(unit, unit).NameSpace != string.Empty)
                    {
                        usings.Add(NodeGenerator.GetSingleDecorator(unit, unit).NameSpace);
                        NodeGenerator.GetSingleDecorator(unit, unit).hasNamespace = true;
                    }
                }
            }

            foreach (var method in Data.methods)
            {
                foreach (Unit unit in method.graph.units)
                {
                    if (!usings.Contains(NodeGenerator.GetSingleDecorator(unit, unit).NameSpace) && NodeGenerator.GetSingleDecorator(unit, unit).NameSpace != string.Empty)
                    {
                        usings.Add(NodeGenerator.GetSingleDecorator(unit, unit).NameSpace);
                        NodeGenerator.GetSingleDecorator(unit, unit).hasNamespace = true;
                    }
                }
            }

            var @class = ClassGenerator.Class(RootAccessModifier.Public, ClassModifier.None, Data.title.LegalMemberName(), modifiedClassName.ToString().Replace(" ", "") + args, Data.unitType.Namespace, usings);
            if (Data.category.Length > 0) @class.AddAttribute(AttributeGenerator.Attribute<UnitCategory>().AddParameter(Data.category));
            if (Data.title.Length > 0) @class.AddAttribute(AttributeGenerator.Attribute<UnitTitleAttribute>().AddParameter(Data.title));
            if (Data.TypeIcon != null) @class.AddAttribute(AttributeGenerator.Attribute<TypeIconAttribute>().AddParameter(Data.TypeIcon.type));

            AddDefinitionMethod(Data);

            if (!Initalized)
            {
                Initalized = true;
                Data.onValueChanged += (type) =>
                {
                    RemoveProperties(Data);
                    UpdateProperties(Data, type);
                    /*RemoveMethods(Data);
                      UpdateMethods(Data, type);*/
                };
            }

            for (int i = 0; i < Data.constructors.Count; i++)
            {
                var constructor = ConstructorGenerator.Constructor(Data.constructors[i].scope, Data.constructors[i].modifier, Data.title.LegalMemberName());
                if (Data.constructors[i].graph.units.Count > 0)
                {
                    var unit = Data.constructors[i].graph.units[0] as FunctionNode;
                    var data = new ControlGenerationData();
                    for (int item = 0; item < Data.variables.Count; item++)
                    {
                        data.AddLocalName(Data.variables[item].name);
                    }

                    constructor.Body(FunctionNodeGenerator.GetSingleDecorator(unit, unit).GenerateControl(null, data, 0));

                    for (int pIndex = 0; pIndex < Data.constructors[i].parameters.Count; pIndex++)
                    {
                        if (!string.IsNullOrEmpty(Data.constructors[i].parameters[pIndex].name)) constructor.AddParameter(false, ParameterGenerator.Parameter(Data.constructors[i].parameters[pIndex].name, Data.constructors[i].parameters[pIndex].type, Libraries.CSharp.ParameterModifier.None));
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
                            var data = new ControlGenerationData();
                            for (int item = 0; item < Data.variables.Count; item++)
                            {
                                data.AddLocalName(Data.variables[item].name);
                            }

                            data.returns = Data.variables[i].type;
                            property.MultiStatementGetter(AccessModifier.Public, NodeGenerator.GetSingleDecorator(Data.variables[i].getter.graph.units[0] as Unit, Data.variables[i].getter.graph.units[0] as Unit)
                            .GenerateControl(null, data, 0));
                        }

                        if (Data.variables[i].set)
                        {
                            var data = new ControlGenerationData();
                            for (int item = 0; item < Data.variables.Count; item++)
                            {
                                data.AddLocalName(Data.variables[item].name);
                            }
                            property.MultiStatementSetter(AccessModifier.Public, NodeGenerator.GetSingleDecorator(Data.variables[i].setter.graph.units[0] as Unit, Data.variables[i].setter.graph.units[0] as Unit)
                            .GenerateControl(null, data, 0));
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
                        var data = new ControlGenerationData();
                        for (int item = 0; item < Data.variables.Count; item++)
                        {
                            data.AddLocalName(Data.variables[item].name);
                        }
                        method.Body(FunctionNodeGenerator.GetSingleDecorator(unit, unit).GenerateControl(null, data, 0));

                        for (int pIndex = 0; pIndex < Data.methods[i].parameters.Count; pIndex++)
                        {
                            if (!string.IsNullOrEmpty(Data.methods[i].parameters[pIndex].name)) method.AddParameter(ParameterGenerator.Parameter(Data.methods[i].parameters[pIndex].name, Data.methods[i].parameters[pIndex].type, Libraries.CSharp.ParameterModifier.None));
                        }
                    }

                    @class.AddMethod(method);
                }
            }

            @namespace.AddClass(@class);
            @namespace.@namespace = Data.Namespace;
            @namespace.hideBrackets = false;

            return @class;
        }
    }
}
