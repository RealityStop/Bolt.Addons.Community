using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Collections.Generic;
using static Unity.VisualScripting.Community.Libraries.Humility.HUMType_Children;
using Unity.VisualScripting.Community.CSharp;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.InterfacePropertyGenerator")]
    public sealed class InterfacePropertyGenerator : ConstructGenerator
    {
        public string name;
        public Type type = typeof(object);
        public bool withNamespace;
        public string stringNamespace;
        public string stringType;
        public bool isStringType;
        public HighlightType highlightType;
        public string get;
        public string set;
        public List<string> definedUsings = new List<string>();

        public override void Generate(CodeWriter writer, ControlGenerationData data)
        {
            if (isStringType)
            {
                if (withNamespace)
                {
                    writer.WriteIndented(stringNamespace.SlashesToPeriods().OnNotNullOrEmpty(string.Empty) + stringType.WithHighlight(highlightType));
                }
                else
                {
                    writer.WriteIndented(stringType.WithHighlight(highlightType));
                }
            }
            else
            {
                writer.WriteIndented();
                writer.Write(type);
            }

            writer.Write(" " + name.LegalMemberName().VariableHighlight());

            writer.Write(" " + "{ " + get + set + "}").NewLine();
        }

        private InterfacePropertyGenerator() { }

        public static InterfacePropertyGenerator Property(string name, Type type, bool get, bool set, HighlightType highlightType = HighlightType.Type)
        {
            var interfaceProp = new InterfacePropertyGenerator();
            interfaceProp.name = name;
            interfaceProp.type = type;
            interfaceProp.isStringType = false;
            interfaceProp.highlightType = highlightType;
            interfaceProp.get = (get ? "get".ConstructHighlight() + "; " : string.Empty);
            interfaceProp.set = (set ? "set".ConstructHighlight() + "; " : string.Empty);
            return interfaceProp;
        }

        public static InterfacePropertyGenerator Property(string name, string stringNamespace, string type, bool get, bool set, bool withNamespace = true, HighlightType highlightType = HighlightType.Type)
        {
            var interfaceProp = new InterfacePropertyGenerator();
            interfaceProp.name = name;
            interfaceProp.withNamespace = withNamespace;
            interfaceProp.stringNamespace = stringNamespace;
            interfaceProp.stringType = type;
            interfaceProp.isStringType = true;
            interfaceProp.highlightType = highlightType;
            interfaceProp.get = (get ? "get".ConstructHighlight() + "; " : string.Empty);
            interfaceProp.set = (set ? "set".ConstructHighlight() + "; " : string.Empty);
            return interfaceProp;
        }

        public override List<string> Usings()
        {
            var usings = new List<string>();
            if (type != null)
            {
                if (!type.Is().PrimitiveStringOrVoid()) usings.Add(type.Namespace);

            }
            else if (!string.IsNullOrEmpty(stringNamespace))
            {
                usings.Add(stringNamespace);
            }
            return usings;
        }
    }
}